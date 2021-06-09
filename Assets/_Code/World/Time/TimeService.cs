using System;
using System.Collections.Generic;
using Aqua.Debugging;
using BeauUtil;
using BeauUtil.Debugger;
using UnityEngine;

namespace Aqua
{
    public class TimeService : ServiceBehaviour, IPauseable
    {
        private const float RealMinutesPerDay = 60 * 24;
        private const float DefaultMinutesPerDay = 12;

        #region Inspector

        [SerializeField] private float m_MinutesPerDay = DefaultMinutesPerDay;

        [Header("Defaults")]
        [SerializeField, Range(0, 23.99f)] private float m_StartingTime = 7f;
        [SerializeField, AutoEnum] private DayName m_StartingDay = DayName.Tuesday;

        #endregion // Inspector

        [NonSerialized] private float m_CurrentTime;
        [NonSerialized] private InGameTime m_FullTime;
        [NonSerialized] private ushort m_TotalDays;
        [NonSerialized] private DayName m_CurrentDayName;
        [NonSerialized] private bool m_Paused;
        [NonSerialized] private bool m_TimeCanFlow;

        [NonSerialized] private TimeMode m_TimeMode;
        [NonSerialized] private float m_QueuedAdvance;
        [NonSerialized] private float m_QueuedSet = -1;

        public InGameTime Current { get { return m_FullTime; } }
        public TimeMode Mode { get { return m_TimeMode; } }

        public ushort StartingTime() { return InGameTime.HoursToTicks(m_StartingTime); }
        public DayName StartingDayName() { return m_StartingDay; }

        private void LateUpdate()
        {
            if (m_Paused || !m_TimeCanFlow || Services.Script.IsCutscene() || Services.State.IsLoadingScene() || Services.UI.IsTransitioning())
                return;

            InGameTime prevTime = m_FullTime;

            switch(m_TimeMode)
            {
                case TimeMode.Normal:
                    AdvanceTime(Time.deltaTime, m_MinutesPerDay);
                    break;

                case TimeMode.Realtime:
                    AdvanceTime(Time.deltaTime, RealMinutesPerDay);
                    break;
            }

            if (m_TimeMode >= TimeMode.FreezeAt0 && m_TimeMode <= TimeMode.FreezeAt22)
            {
                SetTime((int) (m_TimeMode - TimeMode.FreezeAt0) * 2, 0);
            }

            PostUpdateTime(prevTime);
        }

        private float ConsumeQueuedAdvance()
        {
            float ticks = m_QueuedAdvance;
            m_QueuedAdvance = 0;
            
            if (m_QueuedSet >= 0)
            {
                float diff = m_QueuedSet - m_CurrentTime;
                if (diff < 0)
                    diff += InGameTime.TicksPerDay;
                ticks += diff;
                m_QueuedSet = -1;
            }

            return ticks;
        }

        private void AdvanceTime(float inDeltaTime, float inMinutesPerDay)
        {
            float queuedAdvance = ConsumeQueuedAdvance();
            float advancedTicks = queuedAdvance > 0 ? queuedAdvance : InGameTime.RealSecondsToTicks(inDeltaTime, inMinutesPerDay);
            m_CurrentTime += advancedTicks;
        }

        private void SetTime(int inHour, int inMinutes)
        {
            m_CurrentTime = InGameTime.ClockToTicks(inHour, inMinutes);
            ConsumeQueuedAdvance();
        }

        private void PostUpdateTime(InGameTime inPrev)
        {
            while (m_CurrentTime >= InGameTime.TicksPerDay)
            {
                m_CurrentTime -= InGameTime.TicksPerDay;
                m_TotalDays++;
                m_CurrentDayName = (DayName) (((int) m_CurrentDayName + 1) % InGameTime.MaxDayNames);
            }

            InGameTime newTime = m_FullTime = new InGameTime((ushort) m_CurrentTime, m_TotalDays, m_CurrentDayName);

            UpdateTimeElements(inPrev, newTime);
            DispatchTimeChangeEvents(inPrev, newTime);
        }

        private void UpdateTimeElements(InGameTime inPrev, InGameTime inNew)
        {
            
        }

        static private void DispatchTimeChangeEvents(InGameTime inPrev, InGameTime inNew)
        {
            if (inNew.Day != inPrev.Day)
            {
                DebugService.Log(LogMask.Time, "[TimeService] Day changed to {0} for time {1}", inNew.Day, inNew);
                Services.Events.Dispatch(GameEvents.TimeDayChanged, inNew);
            }

            if (inNew.Phase != inPrev.Phase)
            {
                DebugService.Log(LogMask.Time, "[TimeService] Day phase changed to {0} for time {1}", inNew.Phase, inNew);
                Services.Events.Dispatch(GameEvents.TimePhaseChanged, inNew);
            }

            if (inNew.IsDay != inPrev.IsDay)
            {
                DebugService.Log(LogMask.Time, "[TimeService] Day day/night changed to {0} for time {1}", inNew.IsDay ? "Day" : "Night", inNew);
                Services.Events.Dispatch(GameEvents.TimeDayNightChanged, inNew);
            }
        }
    
        #region IService

        protected override void Initialize()
        {
            Services.Events.Register(GameEvents.ProfileUnloaded, OnProfileUnload, this)
                .Register(GameEvents.ProfileLoaded, OnProfileLoaded, this)
                .Register(GameEvents.ProfileStarted, OnProfileStarted, this);
        }

        protected override void Shutdown()
        {
            Services.Events?.DeregisterAll(this);
        }

        #endregion // IService

        #region Handlers

        private void OnProfileUnload()
        {
            m_TimeCanFlow = false;
        }

        private void OnProfileLoaded()
        {
            var mapData = Services.Data.Profile.Map;

            m_CurrentTime = mapData.TimeOfDay;
            m_TotalDays = mapData.TotalDays;
            m_CurrentDayName = mapData.CurrentDay;
            m_TimeMode = mapData.TimeMode;
            m_TimeCanFlow = false;
        }

        private void OnProfileStarted()
        {
            m_TimeCanFlow = true;
        }

        #endregion // Handlers

        #region IPauseable

        bool IPauseable.IsPaused()
        {
            return m_Paused;
        }

        void IPauseable.Pause()
        {
            m_Paused = true;
        }

        void IPauseable.Resume()
        {
            m_Paused = false;
        }

        #endregion // IPauseable
    }
}