using System;
using System.Collections.Generic;
using Aqua.Debugging;
using BeauData;
using BeauUtil;
using BeauUtil.Debugger;
using UnityEngine;

namespace Aqua.Profile
{
    public class MapData : IProfileChunk, ISerializedVersion
    {
        private StringHash32 m_CurrentStationId;
        private StringHash32 m_CurrentMapId;
        private StringHash32 m_CurrentMapEntranceId;

        private HashSet<StringHash32> m_UnlockedStationIds = new HashSet<StringHash32>();
        private HashSet<StringHash32> m_UnlockedSiteIds = new HashSet<StringHash32>();

        public ushort TimeOfDay;
        public ushort TotalDays;
        public TimeMode TimeMode;
        
        private bool m_HasChanges;

        #region Current Station

        public bool SetCurrentStationId(StringHash32 inNewStationId)
        {
            if (m_CurrentStationId != inNewStationId)
            {
                m_CurrentStationId = inNewStationId;
                m_HasChanges = true;
                Services.Events.QueueForDispatch(GameEvents.StationChanged, inNewStationId);
                return true;
            }

            return false;
        }

        public StringHash32 CurrentStationId()
        {
            return m_CurrentStationId;
        }

        #endregion // Current Station

        #region Unlocked Stations

        public bool IsStationUnlocked(StringHash32 inStationId)
        {
            Assert.True(Services.Assets.Map.HasId(inStationId), "Unknown station id '{0}'", inStationId);
            return m_UnlockedStationIds.Contains(inStationId);
        }

        public bool UnlockStation(StringHash32 inStationId)
        {
            Assert.True(Services.Assets.Map.HasId(inStationId), "Unknown station id '{0}'", inStationId);
            if (m_UnlockedStationIds.Add(inStationId))
            {
                m_HasChanges = true;
                return true;
            }

            return false;
        }

        public bool LockStation(StringHash32 inStationId)
        {
            Assert.True(Services.Assets.Map.HasId(inStationId), "Unknown station id '{0}'", inStationId);
            if (m_UnlockedStationIds.Remove(inStationId))
            {
                m_HasChanges = true;
                return true;
            }

            return false;
        }

        #endregion // Unlocked Stations

        public void SetDefaults()
        {
            m_CurrentStationId = Services.Assets.Map.DefaultStationId();
            m_UnlockedStationIds.Add(m_CurrentStationId);

            TimeOfDay = Services.Time.StartingTime();
            TotalDays = 0;
        }

        public void FullSync()
        {
            SyncMapId();
            SyncTime();
        }

        public bool SyncMapId()
        {
            StringHash32 mapId = MapDB.LookupCurrentMap();
            if (!mapId.IsEmpty && m_CurrentMapId != mapId)
            {
                m_CurrentMapId = mapId;
                m_HasChanges = true;
                DebugService.Log(LogMask.DataService, "[MapData] Current map id is '{0}' with entrance '{1}'", m_CurrentMapId, m_CurrentMapEntranceId);
                return true;
            }

            return false;
        }

        public bool SetEntranceId(StringHash32 inEntranceId)
        {
            if (inEntranceId != m_CurrentMapEntranceId)
            {
                m_CurrentMapEntranceId = inEntranceId;
                m_HasChanges = true;
                DebugService.Log(LogMask.DataService, "[MapData] Current map entrance is '{0}'", m_CurrentMapEntranceId);
                return true;
            }

            return false;
        }

        public bool SyncTime()
        {
            Services.Time.ProcessQueuedChanges();

            GTDate currentTime = Services.Time.Current;
            if (currentTime.Day != TotalDays || currentTime.Ticks != TimeOfDay)
            {
                TotalDays = (ushort) currentTime.Day;
                TimeOfDay = (ushort) currentTime.Ticks;
                m_HasChanges = true;
                DebugService.Log(LogMask.DataService, "[MapData] Current time id is '{0}'", new GTDate(TimeOfDay, TotalDays));
                return true;
            }

            return false;
        }

        public StringHash32 SavedSceneId() { return m_CurrentMapId; }
        public StringHash32 SavedSceneLocationId() { return m_CurrentMapEntranceId; }

        #region IProfileChunk

        ushort ISerializedVersion.Version { get { return 2; } }

        void ISerializedObject.Serialize(Serializer ioSerializer)
        {
            ioSerializer.UInt32Proxy("stationId", ref m_CurrentStationId);
            ioSerializer.UInt32ProxySet("unlockedStations", ref m_UnlockedStationIds);
            ioSerializer.UInt32Proxy("currentMapId", ref m_CurrentMapId);

            if (ioSerializer.ObjectVersion >= 2)
            {
                ioSerializer.UInt32Proxy("mapLocationId", ref m_CurrentMapEntranceId);
                ioSerializer.Serialize("timeOfDay", ref TimeOfDay);
                ioSerializer.Serialize("days", ref TotalDays);
                ioSerializer.Enum("timeMode", ref TimeMode);
            }
        }

        public bool HasChanges()
        {
            return m_HasChanges;
        }

        public void MarkChangesPersisted()
        {
            m_HasChanges = false;
        }

        #endregion // IProfileChunk
    }
}