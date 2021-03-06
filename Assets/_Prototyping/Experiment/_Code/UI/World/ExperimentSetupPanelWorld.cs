using UnityEngine;
using BeauUtil.UI;
using System;
using BeauRoutine;
using Aqua;
using BeauUtil;
using Aqua.Animation;

namespace ProtoAqua.Experiment
{
    public class ExperimentSetupPanelWorld : MonoBehaviour
    {
        static private readonly StringHash32 NormalTooltip = "experiment.device.unstarted.tooltip";
        static private readonly StringHash32 InProgressTooltip = "experiment.device.inProgress.tooltip";

        #region Inspector

        [SerializeField] private PointerListener m_Proxy = null;
        [SerializeField] private SpriteAnimator m_Animation = null;
        [SerializeField] private CursorInteractionHint m_CursorHint = null;

        [SerializeField] private Vector3 m_ExperimentOffset = default(Vector3);

        [SerializeField] private SpriteAnimation m_InactiveAnim = null;
        [SerializeField] private SpriteAnimation m_ActiveAnim = null;

        #endregion // Inspector

        [NonSerialized] private Vector3 m_OriginalPosition;

        #region Unity Events

        private void Awake()
        {
            m_Proxy.onClick.AddListener((e) => Services.Events.Dispatch(ExperimentEvents.SetupPanelOn));

            Services.Events.Register(ExperimentEvents.SetupPanelOn, OnPanelOn, this)
                .Register(ExperimentEvents.SetupPanelOff, OnPanelOff, this)
                .Register(ExperimentEvents.ExperimentBegin, OnExperimentBegin, this)
                .Register(ExperimentEvents.ExperimentTeardown, OnExperimentTeardown, this);

            m_OriginalPosition = transform.localPosition;

            m_Animation.Play(m_InactiveAnim);
            m_CursorHint.TooltipId = NormalTooltip;
        }

        private void OnDestroy()
        {
            Services.Events?.DeregisterAll(this);
        }

        #endregion // Unity Events

        private void OnPanelOn()
        {
            m_Animation.Pause();
        }

        private void OnPanelOff()
        {
            m_Animation.Restart();
        }

        private void OnExperimentBegin()
        {
            m_Animation.Play(m_ActiveAnim);
            Routine.Start(this, transform.MoveTo(m_OriginalPosition + m_ExperimentOffset, 0.5f, Axis.XYZ, Space.Self).Ease(Curve.CubeOut));
            m_CursorHint.TooltipId = InProgressTooltip;
        }

        private void OnExperimentTeardown()
        {
            m_Animation.Play(m_InactiveAnim);
            transform.localPosition = m_OriginalPosition;
            m_CursorHint.TooltipId = NormalTooltip;
        }
    }
}