using UnityEngine;
using BeauUtil;
using UnityEngine.UI;
using BeauPools;
using System;
using BeauUtil.Debugger;

namespace Aqua
{
    public class FactPools : MonoBehaviour
    {
        #region Types

        [Serializable] private class BehaviorPool : SerializablePool<BehaviorFactDisplay> { }
        [Serializable] private class ModelPool : SerializablePool<ModelFactDisplay> { }
        [Serializable] private class StatePool : SerializablePool<StateFactDisplay> { }
        [Serializable] private class PropertyPool : SerializablePool<WaterPropertyFactDisplay> { }

        #endregion // Types

        #region Inspector

        [SerializeField] private BehaviorPool m_BehaviorFacts = null;
        [SerializeField] private ModelPool m_ModelFacts = null;
        [SerializeField] private StatePool m_StateFacts = null;
        [SerializeField] private PropertyPool m_PropertyFacts = null;

        [SerializeField] private Transform m_TransformPool = null;
        [SerializeField] private Transform m_TransformTarget = null;

        #endregion // Inspector

        [NonSerialized] private bool m_ConfiguredPools;

        private void Awake()
        {
            ConfigurePoolTransforms();
        }

        private void OnDisable()
        {
            FreeAll();
        }

        private void ConfigurePoolTransforms()
        {
            if (m_ConfiguredPools)
                return;

            m_BehaviorFacts.ConfigureTransforms(m_TransformPool, m_TransformTarget, false);
            m_ModelFacts.ConfigureTransforms(m_TransformPool, m_TransformTarget, false);
            m_StateFacts.ConfigureTransforms(m_TransformPool, m_TransformTarget, false);
            m_PropertyFacts.ConfigureTransforms(m_TransformPool, m_TransformTarget, false);

            m_ConfiguredPools = true;
        }

        public void FreeAll()
        {
            m_BehaviorFacts.Reset();
            m_ModelFacts.Reset();
            m_StateFacts.Reset();
            m_PropertyFacts.Reset();
        }

        public MonoBehaviour Alloc(BFBase inFact, BestiaryDesc inReference)
        {
            ConfigurePoolTransforms();

            BFBehavior behavior = inFact as BFBehavior;
            if (behavior != null)
            {
                BehaviorFactDisplay display = m_BehaviorFacts.Alloc();
                display.Populate(behavior, inReference);
                return display;
            }

            BFModel model = inFact as BFModel;
            if (model != null)
            {
                ModelFactDisplay display = m_ModelFacts.Alloc();
                display.Populate(model);
                return display;
            }

            BFState state = inFact as BFState;
            if (state != null)
            {
                StateFactDisplay display = m_StateFacts.Alloc();
                display.Populate(state);
                return display;
            }

            BFWaterProperty waterProp = inFact as BFWaterProperty;
            if (waterProp != null)
            {
                WaterPropertyFactDisplay display = m_PropertyFacts.Alloc();
                display.Populate(waterProp);
                return display;
            }

            Assert.True(false, "Unable to find suitable fact");
            return null;
        }

        public MonoBehaviour Alloc(BFBase inFact, BestiaryDesc inReference, Transform inParent)
        {
            ConfigurePoolTransforms();

            BFBehavior behavior = inFact as BFBehavior;
            if (behavior != null)
            {
                BehaviorFactDisplay display = m_BehaviorFacts.Alloc(inParent);
                display.Populate(behavior, inReference);
                return display;
            }

            BFModel model = inFact as BFModel;
            if (model != null)
            {
                ModelFactDisplay display = m_ModelFacts.Alloc(inParent);
                display.Populate(model);
                return display;
            }

            BFState state = inFact as BFState;
            if (state != null)
            {
                StateFactDisplay display = m_StateFacts.Alloc(inParent);
                display.Populate(state);
                return display;
            }

            BFWaterProperty waterProp = inFact as BFWaterProperty;
            if (waterProp != null)
            {
                WaterPropertyFactDisplay display = m_PropertyFacts.Alloc(inParent);
                display.Populate(waterProp);
                return display;
            }

            Assert.True(false, "Unable to find suitable fact");
            return null;
        }

        public BehaviorFactDisplay Alloc(BFBehavior inFact, BestiaryDesc inReference)
        {
            ConfigurePoolTransforms();
            BehaviorFactDisplay display = m_BehaviorFacts.Alloc();
            display.Populate(inFact, inReference);
            return display;
        }

        public BehaviorFactDisplay Alloc(BFBehavior inFact, BestiaryDesc inReference, Transform inParent)
        {
            ConfigurePoolTransforms();
            BehaviorFactDisplay display = m_BehaviorFacts.Alloc(inParent);
            display.Populate(inFact, inReference);
            return display;
        }

        public ModelFactDisplay Alloc(BFModel inFact)
        {
            ConfigurePoolTransforms();
            ModelFactDisplay display = m_ModelFacts.Alloc();
            display.Populate(inFact);
            return display;
        }

        public ModelFactDisplay Alloc(BFModel inFact, Transform inParent)
        {
            ConfigurePoolTransforms();
            ModelFactDisplay display = m_ModelFacts.Alloc(inParent);
            display.Populate(inFact);
            return display;
        }

        public StateFactDisplay Alloc(BFState inFact)
        {
            ConfigurePoolTransforms();
            StateFactDisplay display = m_StateFacts.Alloc();
            display.Populate(inFact);
            return display;
        }

        public StateFactDisplay Alloc(BFState inFact, Transform inParent)
        {
            ConfigurePoolTransforms();
            StateFactDisplay display = m_StateFacts.Alloc(inParent);
            display.Populate(inFact);
            return display;
        }

        public WaterPropertyFactDisplay Alloc(BFWaterProperty inFact)
        {
            ConfigurePoolTransforms();
            WaterPropertyFactDisplay display = m_PropertyFacts.Alloc();
            display.Populate(inFact);
            return display;
        }

        public WaterPropertyFactDisplay Alloc(BFWaterProperty inFact, Transform inParent)
        {
            ConfigurePoolTransforms();
            WaterPropertyFactDisplay display = m_PropertyFacts.Alloc(inParent);
            display.Populate(inFact);
            return display;
        }
    }
}