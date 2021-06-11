using System;
using UnityEngine;
using BeauUtil;
using AquaAudio;
using BeauRoutine;
using System.Collections;
using Aqua;
using System.Collections.Generic;

namespace ProtoAqua.Experiment
{
    public class FoundationalTank : ExperimentTank
    {
        #region Inspector

        [SerializeField] private float m_SpawnDelay = 0.05f;

        [SerializeField] private float m_ClimbOffset = 0.15f;

        #endregion // Inspector

        [NonSerialized] private AudioHandle m_AudioLoop;
        [NonSerialized] private HashSet<StringHash32> m_ObservedBehaviors = new HashSet<StringHash32>();

        [NonSerialized] private Routine m_IdleRoutine;
        [NonSerialized] private float m_IdleDuration = 0;

        #region Basic Functions
        protected override void Awake()
        {
            base.Awake();

            m_BaseInput.OnInputDisabled.AddListener(() => {
                m_IdleRoutine.Pause();
            });
            m_BaseInput.OnInputEnabled.AddListener(() => {
                m_IdleRoutine.Resume();
                m_IdleDuration /= 2;
            });
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            Services.Events.Register<StringHash32>(ExperimentEvents.SetupAddActor, SetupAddActor, this)
                .Register<StringHash32>(ExperimentEvents.SetupRemoveActor, SetupRemoveActor, this)
                .Register<StringHash32>(ExperimentEvents.BehaviorAddedToLog, OnBehaviorRecorded, this)
                .Register(ExperimentEvents.AttemptObserveBehavior, ResetIdle, this);

            m_AudioLoop = Services.Audio.PostEvent("tank_water_loop");
        }

        protected override void OnDisable()
        {
            base.OnEnable();

            Services.Events?.DeregisterAll(this);

            m_AudioLoop.Stop();
        }

        public override void OnExperimentStart()
        {
            base.OnExperimentStart();

            ResetIdle();
            m_IdleRoutine.Replace(this, IdleTimer());
        }

        public override void OnExperimentEnd()
        {
            m_IdleRoutine.Stop();
            m_ObservedBehaviors.Clear();

            base.OnExperimentEnd();
        }

        public override void GenerateResult(ExperimentResultData ioData)
        {
            base.GenerateResult(ioData);

            foreach(var fact in m_ObservedBehaviors)
                ioData.NewFactIds.Add(fact);
        }

        private IEnumerator IdleTimer()
        {
            while(true)
            {
                m_IdleDuration += Routine.DeltaTime;
                if (m_IdleDuration >= 30)
                {
                    m_IdleDuration = 0;
                    Services.Script.TriggerResponse(ExperimentTriggers.ExperimentIdle);
                }

                yield return null;
            }
        }

        public override bool TryHandle(ExperimentSetupData inSelection)
        {
            if (inSelection.Tank == TankType.Foundational)
            {
                gameObject.SetActive(true);
                return true;
            }

            return false;
        }

        private void ResetIdle()
        {
            m_IdleDuration = 0;
        }

        private void OnBehaviorRecorded(StringHash32 inBehaviorId)
        {
            m_ObservedBehaviors.Add(inBehaviorId);
        }

        #endregion // Basic Functions

        #region Actors

        private void SetupAddActor(StringHash32 inActorId)
        {
            int spawnCount = GetSettings().GetSpawnCount(m_TankType, inActorId);
            while(spawnCount-- > 0)
            {
                ActorCtrl actor = ExperimentServices.Actors.Pools.Alloc(inActorId, m_ActorRoot);
                actor.Nav.Helper = m_ActorNavHelper;
                actor.Nav.Spawn(spawnCount * RNG.Instance.NextFloat(0.8f, 1.2f) * m_SpawnDelay);
            }
        }

        private void SetupRemoveActor(StringHash32 inActorId)
        {
            ExperimentServices.Actors.Pools.Reset(inActorId);
            Services.UI.WorldFaders.Flash(Color.black, 0.2f);
        }

        #endregion // Actors

        #region Helpers

        public float ClimbOffset()
        {
            return m_ClimbOffset;
        }

        #endregion // Helpers

    }
}