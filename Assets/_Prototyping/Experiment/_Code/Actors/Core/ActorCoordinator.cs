using System;
using UnityEngine;
using BeauData;
using BeauUtil;
using AquaAudio;
using BeauRoutine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using BeauPools;
using BeauUtil.Variants;
using Aqua;

namespace ProtoAqua.Experiment
{
    public class ActorCoordinator : ServiceBehaviour
    {
        #region Inspector

        [SerializeField] private ActorTicker m_TimeTicker = null;
        [SerializeField] private ActorTicker m_ThinkTicker = null;
        [SerializeField] private ActorPools m_Pools = null;

        #endregion // Ticker

        [NonSerialized] private readonly List<ActorCtrl> m_AllActors = new List<ActorCtrl>();
        [NonSerialized] private DynamicPool<VariantTable> m_VariantTablePool;
        [NonSerialized] private bool m_Ticking;
        [NonSerialized] private Dictionary<StringHash32, object> m_NamedObjects = new Dictionary<StringHash32, object>();

        [NonSerialized] private uint m_NextId;

        [NonSerialized] private WaterPropertyBlockF32 m_currentWaterState;
        private bool m_currentWaterStateDirty;

        #region Register/Deregister

        public void Register(ActorCtrl inActor)
        {
            m_AllActors.Add(inActor);
        }

        public void Deregister(ActorCtrl inActor)
        {
            m_AllActors.FastRemove(inActor);
        }

        public bool AnyActorsAreAnimating()
        {
            foreach(var actor in m_AllActors)
            {
                if (actor.Nav.IsAnimating())
                    return true;
            }

            return false;
        }

        #endregion // Register/Deregister

        #region Ticking

        private void FixedUpdate()
        {
            if (!m_Ticking)
                return;

            float dt = Time.deltaTime;
            m_TimeTicker.Advance(dt);
            m_ThinkTicker.Advance(dt);

            if (m_currentWaterStateDirty)
            {
                foreach (var actor in m_AllActors)
                {
                    actor.UpdateStressState(m_currentWaterState);
                }
            }

            foreach(var actor in m_AllActors)
            {
                actor.Tick(m_TimeTicker.CurrentTimeMS(), m_ThinkTicker.CurrentTimeMS());
            }
        }

        public void SetCurrentWaterState(WaterPropertyBlockF32 newState)
        {
            m_currentWaterState = newState;
        }

        public void SetCurrentWaterState(BestiaryDesc bestiaryDesc)
        {
            m_currentWaterState =  BestiaryUtils.GenerateInitialState(bestiaryDesc);
        }

        public void BeginTicking()
        {
            m_Ticking = true;
            m_TimeTicker.ResetTime();
            m_ThinkTicker.ResetTime();
        }

        public void PauseTicking()
        {
            m_Ticking = false;
        }

        public void ResumeTicking()
        {
            m_Ticking = true;
        }

        public void StopTicking()
        {
            if (m_Ticking)
            {
                m_Ticking = false;

                foreach(var actor in m_AllActors)
                {
                    actor.Callbacks.OnStopThink?.Invoke();
                }
            }
        }

        public float TimeDuration()
        {
            return m_TimeTicker.CurrentTimeMS() / 1000f;
        }

        #endregion // Ticking

        #region Variant Tables

        public TempAlloc<VariantTable> BorrowTable()
        {
            return m_VariantTablePool.TempAlloc();
        }

        #endregion // Variant Tables

        #region Naming

        public StringHash32 NextId(string inType)
        {
            return string.Format("{0}:{1}", inType, m_NextId++);
        }

        public void RegisterName(StringHash32 inId, object inObject)
        {
            m_NamedObjects.Add(inId, inObject);
        }

        public void DeregisterName(StringHash32 inId)
        {
            m_NamedObjects.Remove(inId);
        }

        public T GetNamedObject<T>(StringHash32 inId)
        {
            object obj;
            m_NamedObjects.TryGetValue(inId, out obj);
            return (T) obj;
        }

        #endregion // Naming

        public ActorPools Pools { get { return m_Pools; } }

        #region Service

        protected override void Initialize()
        {
            m_VariantTablePool = new DynamicPool<VariantTable>(16, (p) => new VariantTable());
            m_VariantTablePool.Prewarm();

            m_VariantTablePool.Config.RegisterOnAlloc((p, o) => o.Name = "TempActor");
            m_VariantTablePool.Config.RegisterOnFree((p, o) => o.Reset());
        }

        protected override void Shutdown()
        {
            m_VariantTablePool.Dispose();
            m_VariantTablePool = null;

            m_AllActors.Clear();
        }

        #endregion // Service
    }
}