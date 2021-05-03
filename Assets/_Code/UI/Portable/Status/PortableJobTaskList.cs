using UnityEngine;
using BeauPools;
using Aqua.Profile;
using UnityEngine.UI;
using BeauUtil;
using BeauRoutine;
using System.Collections;

namespace Aqua
{
    public class PortableJobTaskList : MonoBehaviour
    {
        #region Inspector

        [Header("Task List")]

        [SerializeField] private JobTaskDisplay.Pool m_TaskDisplays = null;
        [SerializeField] private LayoutGroup m_TaskLayout = null;
        [SerializeField] private CanvasGroup m_Group = null;
        [SerializeField] private ScrollRect m_ScrollView = null;

        #endregion // Inspector
        
        #region Events

        private void OnDisable()
        {
            m_TaskDisplays.Reset();
        }

        #endregion // Events
    
        #region Tasks

        public void LoadTasks(JobDesc inJob, JobsData inData)
        {
            m_TaskDisplays.Reset();

            using(PooledList<JobTask> completedTasks = PooledList<JobTask>.Create())
            using(PooledList<JobTask> activeTasks = PooledList<JobTask>.Create())
            {
                foreach(var task in inJob.Tasks())
                {
                    if (inData.IsTaskComplete(task.Id))
                        completedTasks.Add(task);
                    else if (inData.IsTaskActive(task.Id))
                        activeTasks.Add(task);
                }

                foreach(var completedTask in completedTasks)
                {
                    AllocTaskDisplay(completedTask, true);
                }

                foreach(var activeTask in activeTasks)
                {
                    AllocTaskDisplay(activeTask, false);
                }
            }

            m_Group.alpha = 0;
            Routine.Start(this, RebuildLayout());
        }

        private IEnumerator RebuildLayout()
        {
            yield return null;
            
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform) m_TaskLayout.transform);
            m_Group.alpha = 1;
            m_ScrollView.verticalNormalizedPosition = 0;
        }

        private JobTaskDisplay AllocTaskDisplay(JobTask inTask, bool inbComplete)
        {
            JobTaskDisplay taskDisplay = m_TaskDisplays.Alloc();
            taskDisplay.Populate(inTask, inbComplete);

            ColorPalette4 palette = inbComplete ? Services.Assets.Jobs.CompletedPortablePalette() : Services.Assets.Jobs.ActivePortablePalette();
            taskDisplay.Label.Graphic.color = palette.Content;
            taskDisplay.Background.color = palette.Background;

            LayoutRebuilder.ForceRebuildLayoutImmediate(taskDisplay.Root);

            return taskDisplay;
        }

        #endregion // Tasks
    }
}