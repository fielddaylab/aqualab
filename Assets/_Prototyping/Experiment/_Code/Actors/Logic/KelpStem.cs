using System;
using UnityEngine;
using BeauData;
using BeauUtil;
using AquaAudio;
using BeauRoutine;
using System.Collections;
using BeauPools;
using BeauUtil.Variants;
using Aqua;

namespace ProtoAqua.Experiment
{
    public class KelpStem : Climbable, IFoodSource
    {
        #region Inspector

        [SerializeField] private Transform m_Body;
        [SerializeField] private Collider2D m_Collider = null;

        #endregion // Inspector
        private SpriteRenderer m_Spine;
        private StringHash32 m_Id;
        private ActorCtrl m_Parent;

        public void Initialize(ActorCtrl inParent) 
        {
            if(!IsClimbable()) return;

            m_Id = ExperimentServices.Actors.NextId("KelpStem");
            height = inParent.Body.WorldTransform.GetPosition(Axis.Y, Space.World).y;
            root = inParent.Body.WorldTransform.position.x;
            position = m_Spine.transform.position;
            m_Parent = inParent;
        }

        private bool IsClimbable()
        {
            GameObject parent = m_Body.parent.gameObject;
            bool res = false;
            res |= ComponentUtils.HasComponent<BullKelpActor>(parent);
            if(res)  m_Spine = parent.GetComponent<BullKelpActor>().GetSpine();
            res |= ComponentUtils.HasComponent<GiantKelpActor>(parent);
            if(res)  m_Spine = parent.GetComponent<GiantKelpActor>().GetSpine();

            return res;
        }

        public void ResetPosition(Vector3 point) 
        {
            root = point.x;
            position = point;
        }

        Transform IFoodSource.Transform { get { return m_Body; } }

        Collider2D IFoodSource.Collider { get { return m_Collider; } }

        float IFoodSource.EnergyRemaining { get { return 0f; } }

        StringHash32 IFoodSource.Id { get { return m_Id; } }

        ActorCtrl IFoodSource.Parent { get { return m_Parent; } }

        public bool hasSpine() {
            return m_Spine != null;
        }

        bool IFoodSource.HasTag(StringHash32 inTag) {
            return inTag == "KelpStem";
        }

        void IFoodSource.Bite(ActorCtrl inActor, float inBite) {
            return;
        }

        bool IFoodSource.TryGetEatLocation(ActorCtrl inActor, out Transform outTransform, out Vector3 outOffset) {
            outTransform = null;
            outOffset = Vector3.negativeInfinity;
            return false;
        }

    }
}