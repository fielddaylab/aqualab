using System;
using BeauUtil;
using Aqua.Profile;
using BeauUtil.Services;
using BeauUtil.Debugger;
using System.Runtime.InteropServices;

namespace Aqua
{
    [ServiceDependency(typeof(DataService), typeof(EventService))]
    public partial class JobTaskService : ServiceBehaviour
    {
        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        private struct TaskState
        {
            public JobTaskStatus Status;
            public TempList8<ushort> Prerequisites;
            public JobTask Task;
        }

        private enum TaskEventMask : uint
        {
            SceneLoad = 0x01,
            BestiaryUpdate = 0x02,
            StationUpdate = 0x04,
            ScriptNodeSeen = 0x08,
            VariableUpdated = 0x10,
            ObjectScanned = 0x20,
            InventoryUpdated = 0x40,
            ModelUpdated = 0x80
        }

        [NonSerialized] private StringHash32 m_LoadedJobId;
        [NonSerialized] private TaskEventMask m_TaskMask;
        [NonSerialized] private bool m_JobLoading;
        private readonly RingBuffer<TaskState> m_TaskGraph = new RingBuffer<TaskState>(16, RingBufferMode.Expand);
        private readonly RingBuffer<ushort> m_TaskUpdateQueue = new RingBuffer<ushort>(16, RingBufferMode.Expand);

        protected override void Initialize()
        {
            base.Initialize();

            EventService events = Services.Events;
            events.Register<StringHash32>(GameEvents.JobPreload, OnJobPreload, this)
                .Register<StringHash32>(GameEvents.JobUnload, OnJobUnload, this)
                .Register(GameEvents.CutsceneEnd, OnCutsceneEnd, this);

            RegisterTaskEvent(events, GameEvents.SceneLoaded, TaskEventMask.SceneLoad);
            RegisterTaskEvent(events, GameEvents.BestiaryUpdated, TaskEventMask.BestiaryUpdate);
            RegisterTaskEvent(events, GameEvents.StationChanged, TaskEventMask.StationUpdate);
            RegisterTaskEvent(events, GameEvents.ScriptNodeSeen, TaskEventMask.ScriptNodeSeen);
            RegisterTaskEvent(events, GameEvents.VariableSet, TaskEventMask.VariableUpdated);
            RegisterTaskEvent(events, GameEvents.ScanLogUpdated, TaskEventMask.ObjectScanned);
            RegisterTaskEvent(events, GameEvents.InventoryUpdated, TaskEventMask.InventoryUpdated);
            RegisterTaskEvent(events, GameEvents.ModelUpdated, TaskEventMask.ModelUpdated);
        }

        private void RegisterTaskEvent(EventService inService, StringHash32 inEventId, TaskEventMask inMask)
        {
            inService.Register(inEventId, () => ProcessUpdates(inMask), this);
        }

        protected override void Shutdown()
        {
            Services.Events?.DeregisterAll(this);

            base.Shutdown();
        }

        #region Handlers

        private void OnJobPreload(StringHash32 inJobId)
        {
            if (m_LoadedJobId == inJobId)
                return;

            Assert.True(m_TaskUpdateQueue.Count == 0, "Cannot preload while job queue contains entries");

            m_LoadedJobId = inJobId;
            m_TaskGraph.Clear();
            m_TaskMask = 0;

            if (inJobId.IsEmpty)
                return;

            SaveData saveData = Services.Data.Profile;
            
            m_JobLoading = true;

            ConstructTaskGraph(inJobId);
            SyncTaskStateFromProfile(saveData.Jobs);
            ProcessUpdates(0);

            m_JobLoading = false;
        }

        private void OnJobUnload(StringHash32 inJobId)
        {
            if (m_LoadedJobId != inJobId)
                return;

            ProcessUpdateQueue(Services.Data.Profile.Jobs);

            m_LoadedJobId = StringHash32.Null;
            m_TaskGraph.Clear();
            m_TaskMask = 0;
        }

        private void OnCutsceneEnd()
        {
            ProcessUpdateQueue(Services.Data.Profile.Jobs);
        }

        #endregion // Handlers

        #region Tasks

        private void ConstructTaskGraph(StringHash32 inJobId)
        {
            JobDesc job = Services.Assets.Jobs.Get(inJobId);
            ListSlice<JobTask> taskList = job.Tasks();

            // no tasks? no problem
            if (taskList.Length == 0)
                return;

            TaskEventMask eventMask = 0;

            for(int i = 0; i < taskList.Length; i++)
            {
                JobTask task = taskList[i];
                TaskState state;
                state.Task = task;
                state.Status = JobTaskStatus.Inactive;
                state.Prerequisites = new TempList8<ushort>(task.PrerequisiteTaskIndices);

                eventMask |= RetrieveMask(task);

                m_TaskGraph.PushBack(state);
            }

            m_TaskMask = eventMask;
        }

        private void SyncTaskStateFromProfile(JobsData inData)
        {
            for(int i = 0, length = m_TaskGraph.Count; i < length ; i++)
            {
                ref TaskState taskState = ref m_TaskGraph[i];
                taskState.Status = inData.IsTaskComplete(taskState.Task.Id) ? JobTaskStatus.Complete : JobTaskStatus.Inactive;
            }
        }

        private void ProcessUpdates(TaskEventMask inMask)
        {
            if (inMask != 0 && (m_TaskMask & inMask) == 0)
                return;
            
            SaveData saveData = Services.Data.Profile;
            JobsData jobsData = saveData.Jobs;

            ScanForUpdates(saveData, m_TaskUpdateQueue);
            if (!Services.Script.IsCutscene())
            {
                ProcessUpdateQueue(jobsData);
            }
        }

        private void ScanForUpdates(SaveData inData, RingBuffer<ushort> outUpdated)
        {
            // this assumes all tasks are sorted from root to leaf,
            // such that prerequisite tasks are always evaluated before the task that requires them

            for(int taskIdx = 0, taskCount = m_TaskGraph.Count; taskIdx < taskCount; taskIdx++)
            {
                ref TaskState taskState = ref m_TaskGraph[taskIdx];
                JobTaskStatus desiredStatus = GetDesiredState(inData, ref taskState);
                if (desiredStatus != taskState.Status)
                {
                    taskState.Status = desiredStatus;
                    outUpdated.PushBack((ushort) taskIdx);
                }
            }
        }

        private JobTaskStatus GetDesiredState(SaveData inData, ref TaskState inState)
        {
            if (inState.Status == JobTaskStatus.Complete)
                return JobTaskStatus.Complete;

            for(int i = 0, length = inState.Prerequisites.Count; i < length; i++)
            {
                int prereqIndex = inState.Prerequisites[i];
                if (m_TaskGraph[prereqIndex].Status != JobTaskStatus.Complete)
                    return JobTaskStatus.Inactive;
            }

            if (EvaluateTask(inState.Task, inData))
                return JobTaskStatus.Complete;

            return JobTaskStatus.Active;
        }

        private void ProcessUpdateQueue(JobsData inData)
        {
            ushort taskIndex;
            int count = m_TaskUpdateQueue.Count;

            while(m_TaskUpdateQueue.TryPopFront(out taskIndex))
            {
                ref TaskState state = ref m_TaskGraph[taskIndex];
                switch(state.Status)
                {
                    case JobTaskStatus.Active:
                        inData.SetTaskActive(state.Task.Id);
                        break;

                    case JobTaskStatus.Complete:
                        inData.SetTaskComplete(state.Task.Id);
                        break;

                    case JobTaskStatus.Inactive:
                        inData.SetTaskInactive(state.Task.Id);
                        break;
                }
            }

            if (!m_JobLoading && count > 0)
            {
                Services.Events.Dispatch(GameEvents.JobTasksUpdated);
            }
        }

        #endregion // Queue

        #region Evaluation
        
        static private TaskEventMask RetrieveMask(JobTask inTask)
        {
            TaskEventMask mask = 0;
            for(int i = 0, length = inTask.Steps.Length; i < length; i++)
            {
                switch(inTask.Steps[i].Type)
                {
                    case JobStepType.AcquireBestiaryEntry:
                    case JobStepType.AcquireFact:
                    case JobStepType.UpgradeFact:
                        mask |= TaskEventMask.BestiaryUpdate;
                        break;

                    case JobStepType.GotoScene:
                        mask |= TaskEventMask.SceneLoad;
                        break;

                    case JobStepType.GotoStation:
                        mask |= TaskEventMask.StationUpdate;
                        break;

                    case JobStepType.SeeScriptNode:
                        mask |= TaskEventMask.ScriptNodeSeen;
                        break;

                    case JobStepType.EvaluateCondition:
                        mask |= TaskEventMask.VariableUpdated;
                        break;

                    case JobStepType.ScanObject:
                        mask |= TaskEventMask.ObjectScanned;
                        break;

                    case JobStepType.GetItem:
                        mask |= TaskEventMask.InventoryUpdated;
                        break;

                    case JobStepType.AddFactToModel:
                        mask |= TaskEventMask.ModelUpdated;
                        break;
                }
            }

            return mask;
        }

        static private bool EvaluateTask(JobTask inTask, SaveData inData)
        {
            JobStep[] steps = inTask.Steps;
            
            for(int i = 0, length = steps.Length; i < length; i++)
            {
                if (!EvaluateStep(steps[i], inData))
                    return false;
            }

            return true;
        }

        static private bool EvaluateStep(in JobStep inStep, SaveData inData)
        {
            switch(inStep.Type)
            {
                case JobStepType.GetItem:
                    return inData.Inventory.ItemCount(inStep.Target) >= inStep.Amount;

                case JobStepType.AcquireBestiaryEntry:
                    return inData.Bestiary.HasEntity(inStep.Target);

                case JobStepType.AcquireFact:
                    return inData.Bestiary.HasFact(inStep.Target);

                case JobStepType.EvaluateCondition:
                    return Services.Data.CheckConditions(inStep.ConditionString);

                case JobStepType.GotoScene:
                    return SceneHelper.ActiveScene().Name == inStep.Target.Hash();

                case JobStepType.GotoStation:
                    return inData.Map.CurrentStationId() == inStep.Target;

                case JobStepType.ScanObject:
                    return inData.Inventory.WasScanned(inStep.Target);

                case JobStepType.SeeScriptNode:
                    return inData.Script.HasSeen(inStep.Target, Scripting.PersistenceLevel.Profile);

                case JobStepType.AddFactToModel:
                    return inData.Bestiary.IsFactGraphed(inStep.Target);

                case JobStepType.UpgradeFact:
                    if (Services.Assets.Bestiary.IsAutoFact(inStep.Target))
                        return true;
                    
                    if (!inData.Bestiary.HasFact(inStep.Target))
                        return false;
                    
                    BFBase fact = Services.Assets.Bestiary.Fact(inStep.Target);
                    return !fact.HasValue() || inData.Bestiary.GetFact(inStep.Target).Has(PlayerFactFlags.KnowValue);

                default:
                    return true;
            }
        }

        #endregion // Evaluation
    }
}