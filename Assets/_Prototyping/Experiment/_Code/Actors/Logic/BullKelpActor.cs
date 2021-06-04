using System;
using UnityEngine;
using BeauData;
using BeauUtil;
using AquaAudio;
using BeauRoutine;
using System.Collections;
using BeauPools;
using BeauUtil.Variants;
using BeauRoutine.Extensions;

namespace ProtoAqua.Experiment
{
    public class BullKelpActor : ActorModule
    {
        #region Inspector

        [Header("Height")]
        [SerializeField] private FloatRange m_Height = new FloatRange(6);
        [SerializeField] private Transform m_HeightOffset = null;
        [SerializeField] private SpriteRenderer m_SpineRenderer = null;
        [SerializeField] private Transform m_HeightCapOffset = null;
        [SerializeField] private KelpStem m_Stem = null;

        #endregion // Inspector

        [NonSerialized] private KelpLeaf[] m_AllLeaves = null;

        #region Events

        public override void OnConstruct()
        {
            base.OnConstruct();

            Actor.Callbacks.OnCreate = OnCreate;

            m_AllLeaves = GetComponentsInChildren<KelpLeaf>(true);
        }

        private void OnCreate()
        {
            float height = m_Height.Generate(RNG.Instance);
            m_SpineRenderer.flipX = RNG.Instance.NextBool();
            m_SpineRenderer.flipY = RNG.Instance.NextBool();
            
            Vector2 size = m_SpineRenderer.size;
            size.y = height;
            m_SpineRenderer.size = size;

            m_HeightOffset.SetPosition(height * 0.5f, Axis.Y, Space.Self);
            m_HeightCapOffset.SetPosition(height * 0.5f + 1, Axis.Y, Space.Self);

            int leafCount = (int)Math.Floor(RNG.Instance.NextFloat(GetProperty<float>("MinLeafCount", 2f), GetProperty<float>("MaxLeafCount", 4f)));

            float facing = RNG.Instance.Choose(-1, 1);
            for(int i = 0; i < leafCount; ++i)
            {
                float lerp = 1 + RNG.Instance.NextFloat(-0.2f, 0f);
                float leafHeight = (height * lerp);

                m_AllLeaves[i].Initialize(leafHeight, facing, Actor);
                m_AllLeaves[i].gameObject.SetActive(true);
            }

            for(int i = leafCount; i < m_AllLeaves.Length; i++)
            {
                m_AllLeaves[i].gameObject.SetActive(false);
            }
            ((IClimbable) m_Stem).Initialize(Actor);
        }

        public SpriteRenderer GetSpine() {
            return m_SpineRenderer;
        }

        #endregion // Events
    }
}