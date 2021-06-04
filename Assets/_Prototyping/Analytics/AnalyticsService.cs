using System.Collections.Generic;
using System.Runtime.InteropServices;
using Aqua.Scripting;
using Aqua.Title;
using Aqua.WorldMap;
using BeauUtil;
using BeauUtil.Services;
using FieldDay;
using ProtoAqua.Argumentation;
using ProtoAqua.Experiment;
using ProtoAqua.Modeling;
using UnityEngine;

namespace Aqua
{
    [ServiceDependency(typeof(EventService), typeof(ScriptingService))]
    public partial class AnalyticsService : ServiceBehaviour
    {
        #region Inspector

        [SerializeField, Required] private string m_AppId = "AQUALAB";
        [SerializeField] private int m_AppVersion = 1;
        
        #endregion // Inspector

        #region Firebase JS Functions

        [DllImport("__Internal")]
        public static extern void FBStartGameWithUserCode(string userCode);
        [DllImport("__Internal")]
        public static extern void FBAcceptJob(string jobId);
        [DllImport("__Internal")]
        public static extern void FBSwitchJob(string jobId);
        [DllImport("__Internal")]
        public static extern void FBReceiveFact(string factId);
        [DllImport("__Internal")]
        public static extern void FBCompleteJob(string jobId);
        [DllImport("__Internal")]
        public static extern void FBTaskCompleted(string jobId, string taskId);
        [DllImport("__Internal")]
        public static extern void FBBeginExperiment(string jobId, string tankType);
        [DllImport("__Internal")]
        public static extern void FBEndExperiment(string jobId, string tankType, float duration);
        [DllImport("__Internal")]
        public static extern void FBBeginDive(string jobId, string siteId);
        [DllImport("__Internal")]
        public static extern void FBBeginArgument(string jobId);
        [DllImport("__Internal")]
        public static extern void FBBeginModel(string jobId);
        [DllImport("__Internal")]
        public static extern void FBBeginSimulation(string jobId);
        [DllImport("__Internal")]
        public static extern void FBAskForHelp(string nodeId);
        [DllImport("__Internal")]
        public static extern void FBTalkWithGuide(string nodeId);
        [DllImport("__Internal")]
        public static extern void FBChangeRoom(string jobId, string roomId);
        [DllImport("__Internal")]
        public static extern void FBChangeStation(string jobId, string stationId);
        [DllImport("__Internal")]
        public static extern void FBSimulationSyncAchieved(string jobId);
        [DllImport("__Internal")]
        public static extern void FBGuideScriptTriggered(string nodeId);
        [DllImport("__Internal")]
        public static extern void FBArgueValidResponse(string jobId, string nodeId);
        [DllImport("__Internal")]
        public static extern void FBArgueInvalidResponse(string jobId, string nodeId);

        #endregion // Firebase JS Functions

        #region Logging Variables

        private SimpleLog m_Logger;
        private string m_CurrentJobId = "";
        private string m_CurrentTankType = "";

        #endregion // Logging Variables

        #region IService

        protected override void Initialize()
        {
            m_Logger = new SimpleLog(m_AppId, m_AppVersion);
            Services.Events.Register<StringHash32>(GameEvents.JobStarted, LogAcceptJob)
                .Register<StringHash32>(GameEvents.JobSwitched, LogSwitchJob)
                .Register<BestiaryUpdateParams>(GameEvents.BestiaryUpdated, LogReceiveFact)
                .Register<StringHash32>(GameEvents.JobCompleted, LogCompleteJob)
                .Register<StringHash32>(GameEvents.JobTaskCompleted, LogTaskCompleted)
                .Register<TankType>(ExperimentEvents.ExperimentBegin, LogBeginExperiment)
                .Register<ExperimentResultData>(ExperimentEvents.ExperimentRequestSummary, LogEndExperiment)
                .Register<string>(GameEvents.BeginDive, LogBeginDive)
                .Register(GameEvents.BeginArgument, LogBeginArgument)
                .Register(SimulationConsts.Event_Model_Begin, LogBeginModel)
                .Register(SimulationConsts.Event_Simulation_Begin, LogBeginSimulation)
                .Register<StringHash32>(GameEvents.RoomChanged, LogChangeRoom)
                .Register<StringHash32>(WorldMapCtrl.Event_ShipOut, LogChangeStation)
                .Register(SimulationConsts.Event_Simulation_Complete, LogSimulationSyncAchieved)
                .Register<string>(GameEvents.ProfileStarting, OnTitleStart)
                .Register<StringHash32>(ArgumentationEvents.ArgueValidResponse, LogArgueValidResponse)
                .Register<StringHash32>(ArgumentationEvents.ArgueInvalidResponse, LogArgueInvalidResponse);

            Services.Script.OnTargetedThreadStarted += GuideHandler;
        }

        protected override void Shutdown()
        {
            m_Logger?.Flush();
            m_Logger = null;
        }

        #endregion // IService

        #region Log Events

        private void OnTitleStart(string inUserCode)
        {
            if (!string.IsNullOrEmpty(inUserCode))
            {
                #if !UNITY_EDITOR
                FBStartGameWithUserCode(inUserCode);
                #endif
            }
        }

        private void GuideHandler(ScriptThreadHandle inThread)
        {
            if (inThread.TargetId() != GameConsts.Target_Kevin)
            {
                return;
            }

            string nodeId = inThread.RootNodeName();

            if (inThread.TriggerId() == GameTriggers.PartnerTalk)
            {
                LogTalkWithGuide(nodeId);
            }
            else if (inThread.TriggerId() == GameTriggers.RequestPartnerHelp)
            {
                LogAskForHelp(nodeId);
            }
            else
            {
                LogGuideScriptTriggered(nodeId);
            }
        }

        private void LogAcceptJob(StringHash32 jobId)
        {
            #if !UNITY_EDITOR
            if (jobId.IsEmpty)
            {
                m_CurrentJobId = "";
            }
            else
            {
                m_CurrentJobId = Services.Assets.Jobs.Get(jobId).name;
            }

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId }
            };

            m_Logger.Log(new LogEvent(data, "accept_job"));
            FBAcceptJob(m_CurrentJobId);
            #endif
        }

        private void LogSwitchJob(StringHash32 jobId)
        {
            string oldJobId = m_CurrentJobId;

            if (jobId.IsEmpty)
            {
                m_CurrentJobId = "";
            }
            else
            {
                m_CurrentJobId = Services.Assets.Jobs.Get(jobId).name;
            }

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "old_job_id", oldJobId },
                { "new_job_id", m_CurrentJobId }
            };

            m_Logger.Log(new LogEvent(data, "switch_job"));
            //FBSwitchJob(oldJobId, m_CurrentJobId);
        }

        private void LogReceiveFact(BestiaryUpdateParams inParams)
        {
            #if !UNITY_EDITOR
            if (inParams.Type == BestiaryUpdateParams.UpdateType.Fact)
            {
                string parsedFactId = Services.Assets.Bestiary.Fact(inParams.Id).name;
                
                Dictionary<string, string> data = new Dictionary<string ,string>()
                {
                    { "fact_id", parsedFactId }
                };

                m_Logger.Log(new LogEvent(data, "receive_fact"));
                FBReceiveFact(parsedFactId);
            }
            #endif
        }

        private void LogCompleteJob(StringHash32 jobId)
        {
            #if !UNITY_EDITOR
            string parsedJobId = Services.Assets.Jobs.Get(jobId).name;

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", parsedJobId }
            };

            m_Logger.Log(new LogEvent(data, "complete_job"));
            FBCompleteJob(parsedJobId);

            m_CurrentJobId = "";
            #endif
        }

        private void LogTaskCompleted(StringHash32 taskId)
        {
            string parsedTaskId = taskId.ToString();

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "task_id", parsedTaskId }
            };

            m_Logger.Log(new LogEvent(data, "task_completed"));
            //FBTaskCompleted(m_CurrentJobId, parsedTaskId);
        }

        private void LogBeginExperiment(TankType inTankType)
        {
            #if !UNITY_EDITOR
            m_CurrentTankType = inTankType.ToString();

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "tank_type", m_CurrentTankType }
            };

            m_Logger.Log(new LogEvent(data, "begin_experiment"));
            FBBeginExperiment(m_CurrentJobId, m_CurrentTankType);
            #endif
        }

        private void LogEndExperiment(ExperimentResultData result)
        {
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "tank_type", m_CurrentTankType },
                { "duration", result.Duration.ToString() }
            };

            Debug.Log(result.Duration.ToString());

            m_Logger.Log(new LogEvent(data, "end_experiment"));
            //FBEndExperiment(m_CurrentJobId, m_CurrentTankType, result.Duration);
            m_CurrentTankType = "";
        }

        private void LogBeginDive(string inTargetScene)
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "site_id", inTargetScene }
            };

            m_Logger.Log(new LogEvent(data, "begin_dive"));
            FBBeginDive(m_CurrentJobId, inTargetScene);
            #endif
        }

        private void LogBeginArgument()
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId }
            };

            m_Logger.Log(new LogEvent(data, "begin_argument"));
            FBBeginArgument(m_CurrentJobId);
            #endif
        }

        private void LogBeginModel()
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId }
            };

            m_Logger.Log(new LogEvent(data, "begin_model"));
            FBBeginModel(m_CurrentJobId);
            #endif
        }

        private void LogBeginSimulation()
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId }
            };

            m_Logger.Log(new LogEvent(data, "begin_simulation"));
            FBBeginSimulation(m_CurrentJobId);
            #endif
        }

        private void LogAskForHelp(string nodeId)
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "node_id", nodeId }
            };

            m_Logger.Log(new LogEvent(data, "ask_for_help"));
            FBAskForHelp(nodeId);
            #endif
        }

        private void LogTalkWithGuide(string nodeId)
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "node_id", nodeId }
            };

            m_Logger.Log(new LogEvent(data, "talk_with_guide"));
            FBTalkWithGuide(nodeId);
            #endif
        }

        private void LogChangeRoom(StringHash32 roomId)
        {
            string parsedRoomId = roomId.ToString();

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "room_id", parsedRoomId }
            };

            m_Logger.Log(new LogEvent(data, "change_room"));
            //FBChangeRoom(m_CurrentJobId, parsedRoomId);
        }

        private void LogChangeStation(StringHash32 stationId)
        {
            string parsedStationId = stationId.ToString();

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "station_id", parsedStationId }
            };

            m_Logger.Log(new LogEvent(data, "change_station"));
            //FBChangeStation(m_CurrentJobId, parsedStationId);
        }

        private void LogSimulationSyncAchieved()
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId }
            };

            m_Logger.Log(new LogEvent(data, "simulation_sync_achieved"));
            FBSimulationSyncAchieved(m_CurrentJobId);
            #endif
        }

        private void LogGuideScriptTriggered(string nodeId)
        {
            #if !UNITY_EDITOR
            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "node_id", nodeId }
            };

            m_Logger.Log(new LogEvent(data, "guide_script_triggered"));
            FBGuideScriptTriggered(nodeId);
            #endif
        }

        private void LogArgueValidResponse(StringHash32 nodeId)
        {
            string parsedNodeId = nodeId.ToString();

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "node_id", parsedNodeId }
            };

            m_Logger.Log(new LogEvent(data, "argue_valid_response"));
            //FBArgueValidResponse(m_CurrentJobId, parsedNodeId);
        }

        private void LogArgueInvalidResponse(StringHash32 nodeId)
        {
            string parsedNodeId = nodeId.ToString();

            Dictionary<string, string> data = new Dictionary<string, string>()
            {
                { "job_id", m_CurrentJobId },
                { "node_id", parsedNodeId }
            };

            m_Logger.Log(new LogEvent(data, "argue_invalid_response"));
            //FBArgueInvalidResponse(m_CurrentJobId, parsedNodeId);
        }

        #endregion // Log Events
    }
}
