using System;
using UnityEngine;
using BeauUtil;
using AquaAudio;
using BeauRoutine;
using System.Collections;
using Aqua;

namespace ProtoAqua.Experiment
{
    public class StressorTank : ExperimentTank
    {
        #region Inspector

        [SerializeField] private LocText m_Text = null;

        [SerializeField] private float m_SpawnDelay = 0.05f;

        #endregion // Inspector

        [NonSerialized] private AudioHandle m_AudioLoop;

        [NonSerialized] private float m_defAlpha;

        protected override void Awake()
        {
            base.Awake();
        }


        protected override void OnEnable()
        {
            base.OnEnable();

            Services.Events.Register<StringHash32>(ExperimentEvents.SetupAddActor, SetupAddActor, this)
                .Register<StringHash32>(ExperimentEvents.SetupRemoveActor, SetupRemoveActor, this);
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
        }

        public override void OnExperimentEnd()
        {
            m_Text.SetText("");

            base.OnExperimentEnd();
        }

        public override void GenerateResult(ExperimentResultData ioData)
        {
            base.GenerateResult(ioData);

            BestiaryDesc critter = Services.Assets.Bestiary.Get(ioData.Setup.CritterId);
            BFState fact = BestiaryUtils.FindRangeRule(critter, ioData.Setup.PropertyId);

            if (fact != null)
            {
                if (Services.Data.Profile.Bestiary.RegisterFact(fact.Id()))
                {
                    ioData.NewFactIds.Add(fact.Id());
                }
            }
        }

        public override bool TryHandle(ExperimentSetupData inSelection)
        {
            if (inSelection.Tank == TankType.Stressor)
            {
                gameObject.SetActive(true);
                return true;
            }

            return false;
        }

        private void SetupAddActor(StringHash32 inActorId)
        {
            // ActorCtrl actor = ExperimentServices.Actors.Pools.Alloc(inActorId, m_ActorRoot);
            // actor.Nav.Helper = m_ActorNavHelper;
            // actor.Nav.Spawn(0);

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
    }
    
}