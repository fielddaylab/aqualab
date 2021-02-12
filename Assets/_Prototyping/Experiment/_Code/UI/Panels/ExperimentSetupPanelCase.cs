using System;
using UnityEngine;
using AquaAudio;
using BeauRoutine;
using System.Collections;
using BeauRoutine.Extensions;
using System.Collections.Generic;
using UnityEngine.UI;
using Aqua;
using ProtoAqua;

namespace ProtoAqua.Experiment
{
    public class ExperimentSetupPanelCase : BasePanel
    {
        #region Inspector

        [Header("Animation Settings")]
        [SerializeField] private float m_OffscreenY = -660;
        [SerializeField] private TweenSettings m_ToOnAnim = default(TweenSettings);
        [SerializeField] private TweenSettings m_ToOffAnim = default(TweenSettings);

        [Header("Shared Interface")]
        [SerializeField] private CanvasGroup m_SharedGroup = null;
        [SerializeField] private Button m_CloseButton = null;

        [Header("Panels")]
        [SerializeField] private ExperimentSetupSubscreenBoot m_BootScreen = null;
        [SerializeField] private ExperimentSetupSubscreenTank m_TankScreen = null;
        [SerializeField] private ExperimentSetupSubscreenEco m_EcoScreen = null;
        [SerializeField] private ExperimentSetupSubscreenHypothesis m_HypothesisScreen = null;
        [SerializeField] private ExperimentSetupSubscreenActors m_ActorsScreen = null;
        [SerializeField] private ExperimentSetupSubscreenBegin m_BeginExperimentScreen = null;
        [SerializeField] private ExperimentSetupSubscreenInProgress m_InProgressScreen = null;
        [SerializeField] private ExperimentSetupSubscreenSummary m_SummaryScreen = null;

        [SerializeField] private ExperimentSetupSubscreenWaterProp m_PropertyScreen = null;

        #endregion // Inspector

        #region Local Data
        [NonSerialized] private AudioHandle m_Hum;
        [NonSerialized] private ExperimentSetupSubscreen m_CurrentSubscreen;
        [NonSerialized] private Routine m_SwitchSubscreenRoutine;

        [NonSerialized] private SubscreenDirectory m_SubDirectory;

        [NonSerialized] private ExperimentSetupData m_SelectionData;
        [NonSerialized] private bool m_ExperimentSetup = false;
        [NonSerialized] private bool m_ExperimentRunning = false;

        #endregion // Local Data

        #region Core
        protected override void Awake()
        {
            m_SubDirectory = new SubscreenDirectory(
                null, m_ActorsScreen, m_EcoScreen, m_BeginExperimentScreen, m_InProgressScreen, 
                m_SummaryScreen, m_PropertyScreen, m_TankScreen, m_BootScreen);

            m_CloseButton.onClick.AddListener(() => CancelExperiment());

            Services.Events.Register(ExperimentEvents.SetupPanelOn, () => Show(), this)
                .Register(ExperimentEvents.SetupInitialSubmit, OnExperimentSubmitInitial, this)
                .Register(ExperimentEvents.ExperimentTeardown, OnExperimentTeardown, this)
                .Register(ExperimentEvents.ExperimentBegin, OnExperimentBegin, this);

            SetBaseSubscreenChain();

            m_SelectionData = new ExperimentSetupData();
            SetData(m_SelectionData);
        }

        private void Update()
        {
            SetSubScreenChain(m_TankScreen.SelectedTank());
        }

        

        private void SetData(ExperimentSetupData selectData) {
            foreach (var screen in m_SubDirectory.AllSubscreens())
            {
                if (screen != null)
                {
                    if (ExperimentHelper.HasMethod(screen, "SetData"))
                    {
                        screen.SetData(selectData);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            Services.Events?.DeregisterAll(this);
        }

        #endregion // Core

        #region BasePanel

        protected override void OnShow(bool inbInstant)
        {
            m_Hum = Services.Audio.PostEvent("tablet_hum").SetVolume(0).SetVolume(1, 0.5f);
            Services.Data.SetVariable(ExperimentVars.SetupPanelOn, true);
        }

        protected override void OnHide(bool inbInstant)
        {
            if (WasShowing() && Services.Valid)
            {
                m_Hum.Stop(0.5f);
                Services.Events.Dispatch(ExperimentEvents.SetupPanelOff);

                if (!Services.State.IsLoadingScene())
                {
                    Services.Data.SetVariable(ExperimentVars.SetupPanelOn, false);
                    Services.Data.SetVariable(ExperimentVars.SetupPanelScreen, null);
                }
            }
        }

        protected override void InstantTransitionToHide()
        {
            m_RootTransform.gameObject.SetActive(false);
            m_RootTransform.SetAnchorPos(-m_OffscreenY, Axis.Y);
            SetSubscreen(null);
            m_SharedGroup.alpha = 0;
        }

        protected override IEnumerator TransitionToHide()
        {
            SetSubscreen(null);
            yield return 0.1f;
            m_SharedGroup.alpha = 0;
            yield return m_RootTransform.AnchorPosTo(m_OffscreenY, m_ToOffAnim, Axis.Y);
            m_RootTransform.gameObject.SetActive(false);
        }

        protected override void InstantTransitionToShow()
        {
            m_RootTransform.SetAnchorPos(0, Axis.Y);
            m_RootTransform.gameObject.SetActive(true);
            SetSubscreen(GetBootScreen());
            m_SharedGroup.alpha = 1;
        }

        protected override IEnumerator TransitionToShow()
        {
            if (!m_RootTransform.gameObject.activeSelf)
            {
                m_RootTransform.SetAnchorPos(m_OffscreenY, Axis.Y);
                m_RootTransform.gameObject.SetActive(true);
            }

            SetSubscreen(null, false);
            m_SharedGroup.alpha = 0;

            yield return m_RootTransform.AnchorPosTo(0, m_ToOnAnim, Axis.Y);
            
            yield return 0.1f;
            m_SharedGroup.alpha = 1;

            SetSubscreen(GetBootScreen(), false);
        }

        private ExperimentSetupSubscreen GetBootScreen()
        {
            if (m_ExperimentRunning)
                return m_InProgressScreen;
            return m_BootScreen;
        }

        #endregion // BasePanel

        #region Callbacks

        private void OnExperimentSubmit()
        {
            var kevinResponse = Services.Script.TriggerResponse(ExperimentTriggers.TrySubmitHypothesis);
            if (!kevinResponse.IsRunning())
            {
                SetInputState(false);
                if(m_TankScreen.SelectedTank().Equals(TankType.Foundational)) {
                    Routine.Start(this, LoadFoundationalTankExperiment());
                }

                if(m_TankScreen.SelectedTank().Equals(TankType.Stressor)) {
                    RunStressorTankExperimentRoutine();
                }
                
            }
        }

        private void RunStressorTankExperimentRoutine() {
            Routine.Start(this, LoadFoundationalTankExperiment());
            Routine.Start(this, StartStressorTankExperimentRoutine());
            Routine.Start(this, ExitFoundationalTankExperimentRoutine());
        }

        private IEnumerator LoadExperimentRoutine()
        {
            using(var tempFader = Services.UI.ScreenFaders.AllocFader())
            {
                Services.UI.ShowLetterbox();
                yield return tempFader.Object.Show(Color.black, 0.5f);
                Services.Events.Dispatch(ExperimentEvents.SetupInitialSubmit, m_SelectionData);
                m_ExperimentSetup = true;
                yield return 0.2f;
                Services.UI.HideLetterbox();
                SetSubscreen(m_ActorsScreen);
                yield return tempFader.Object.Hide(0.5f, false);
                SetInputState(true);
            }
        }

        private void CancelExperiment()
        {
            bool bExit;
            if (m_CurrentSubscreen != null && m_CurrentSubscreen.ShouldCancelOnExit().HasValue)
                bExit = m_CurrentSubscreen.ShouldCancelOnExit().Value;
            else
                bExit = m_ExperimentSetup && !m_ExperimentRunning;
            
            if (bExit)
            {
                SetInputState(false);
                Routine.Start(this, ExitExperimentRoutine());
            }
            else
            {
                if (m_CurrentSubscreen == m_SummaryScreen)
                {
                    Services.Script.TriggerResponse(ExperimentTriggers.ExperimentFinished);
                }
                Hide();
            }
        }

        private void TryEndExperiment()
        {
            var kevinResponse = Services.Script.TriggerResponse(ExperimentTriggers.TryEndExperiment);
            if (!kevinResponse.IsRunning())
            {
                Services.UI.Popup.AskYesNo("End Experiment?", "Do you want to end the experiment?")
                    .OnComplete((a) => {
                        if (a == PopupPanel.Option_Yes)
                        {
                            SetInputState(false);
                            Routine.Start(this, ExitExperimentRoutine());
                        }
                    });
            }

        }

        private IEnumerator ExitExperimentRoutine()
        {
            using(var tempFader = Services.UI.ScreenFaders.AllocFader())
            {
                bool bWasRunning = m_ExperimentRunning;
                Services.UI.ShowLetterbox();
                yield return tempFader.Object.Show(Color.black, 0.5f);
                if (bWasRunning)
                {
                    ExperimentResultData result = new ExperimentResultData();
                    result.Setup = m_SelectionData.Clone();
                    Services.Events.Dispatch(ExperimentEvents.ExperimentRequestSummary, result);
                    m_SummaryScreen.Populate(result);
                }
                Services.Events.Dispatch(ExperimentEvents.ExperimentTeardown);
                yield return 0.2f;
                Services.UI.HideLetterbox();
                if (!bWasRunning)
                {
                    InstantHide();
                }
                else
                {
                    SetInputState(true);
                    SetSubscreen(m_SummaryScreen);
                }
                yield return tempFader.Object.Hide(0.5f, false);
            }
        }

        private void StartExperiment()
        {
            var kevinResponse = Services.Script.TriggerResponse(ExperimentTriggers.TrySubmitExperiment);
            if (!kevinResponse.IsRunning())
            {
                SetInputState(false);
                Routine.Start(this, StartFoundationalTankExperimentRoutine());
                // Routine.Start(this, StartFoundationalTankExperimentRoutine());
            }

            // if(m_TankScreen.SelectedTank().Equals(TankType.Stressor)) {
            //     OnExperimentBegin();
            //     Routine.Start(this, StressorTankWait());
            //     TryEndExperiment();

            // }
        }

        private IEnumerator StartExperimentRoutine()
        {
            while(ExperimentServices.Actors.AnyActorsAreAnimating())
                yield return null;
            
            Hide();
            yield return 0.25f;
            Services.Events.Dispatch(ExperimentEvents.ExperimentBegin);
        }

        private void OnExperimentSubmitInitial()
        {
            m_ExperimentSetup = true;
        }

        private void OnExperimentBegin()
        {
            m_ExperimentRunning = true;
        }

        private void OnExperimentTeardown()
        {
            m_ExperimentSetup = false;
            m_ExperimentRunning = false;

            m_SelectionData.Reset();

            m_BootScreen.Refresh();

            var sequence = Services.Tweaks.Get<ExperimentSettings>().GetTank(m_TankScreen.SelectedTank()).Sequence;

            foreach(var sEnum in sequence) {
                if(!(sEnum.Equals(ExpSubscreen.None) || sEnum.Equals(ExpSubscreen.Boot))) {
                    m_SubDirectory.GetSubscreen(sEnum).Refresh();
                }
            }

            

            // m_ActorsScreen.Refresh();
            // m_BootScreen.Refresh();
            // m_TankScreen.Refresh();
            // m_HypothesisScreen.Refresh();
            // m_BeginExperimentScreen.Refresh();
            // m_InProgressScreen.Refresh();

            // if(!m_TankScreen.SelectedTank().Equals(TankType.Stressor)) {
            //     m_EcoScreen.Refresh();
            // }
        }

        #endregion // Callbacks

        #region Subscreens

        private void SetSubscreen(ExperimentSetupSubscreen inSubscreen, bool inbBack = false)
        {
            m_SwitchSubscreenRoutine.Replace(this, SwitchSubscreenRoutine(inSubscreen, inbBack)).TryManuallyUpdate(0);
        }

        private IEnumerator SwitchSubscreenRoutine(ExperimentSetupSubscreen inSubscreen, bool inbBack)
        {
            if (m_CurrentSubscreen != inSubscreen)
            {
                ExperimentSetupSubscreen oldSub = m_CurrentSubscreen;
                m_CurrentSubscreen = inSubscreen;
                if (oldSub)
                {
                    oldSub.Hide();
                    if (m_CurrentSubscreen)
                    {
                        yield return 0.2f;
                    }
                }

                if (m_CurrentSubscreen)
                {
                    m_CurrentSubscreen.Show();
                    Services.Audio.PostEvent(inbBack ? "tablet_ui_back" : "tablet_ui_advance");
                }
            }
        }

        private void SetBaseSubscreenChain() {
            m_BootScreen.OnSelectContinue = () => SetSubscreen(m_TankScreen);
            m_BeginExperimentScreen.OnSelectStart = () => StartExperiment();
            m_TankScreen.OnSelectConstruct = () => OnExperimentSubmit();
            m_EcoScreen.OnSelectConstruct = () => OnExperimentSubmit();
            m_TankScreen.OnChange = () => SetSubSequence(m_TankScreen.SelectedTank());
            m_InProgressScreen.OnSelectEnd = () => TryEndExperiment();
        }

        private void SetSubScreenChain(TankType tank) 
        {

            if(!tank.Equals(TankType.None)){
                var sequence = Services.Tweaks.Get<ExperimentSettings>().GetTank(tank).Sequence;
                SetActions(sequence);

            }

        }


        private void SetActions(ExpSubscreen[] sequence) {
            List<ExpSubscreen> bases = new List<ExpSubscreen>(){ExpSubscreen.None, ExpSubscreen.Boot};
            if(sequence != m_SubDirectory.GetSequence()) {
                m_SubDirectory.SetSequence(sequence);
            }
            foreach(var sub in sequence) {
                if(bases.Contains(sub)) continue;

                if(sub.Equals(ExpSubscreen.Tank)) {
                    if(m_SubDirectory.HasNext(sub)) {
                        m_TankScreen.OnSelectContinue = () => SetSubscreen(m_SubDirectory.GetNext(sub));
                    }
                }
                if(sub.Equals(ExpSubscreen.Actor)) {
                    if(m_SubDirectory.HasNext(sub)) {
                        m_ActorsScreen.OnSelectContinue = () => SetSubscreen(m_SubDirectory.GetNext(sub));
                    }
                }
                if(sub.Equals(ExpSubscreen.Begin)) {
                    if(m_SubDirectory.HasPrev(sub)) {
                        m_BeginExperimentScreen.OnSelectBack = () => SetSubscreen(m_SubDirectory.GetPrevious(sub), true);
                    }
                }
                if(sub.Equals(ExpSubscreen.Ecosystem)) {
                    if(m_SubDirectory.HasPrevNext(sub)) {
                        m_EcoScreen.OnSelectContinue = () => SetSubscreen(m_SubDirectory.GetNext(sub));
                        m_EcoScreen.OnSelectBack = () => SetSubscreen(m_SubDirectory.GetPrevious(sub), true);
                    }
                }
                if(sub.Equals(ExpSubscreen.Property)) {
                    if(m_SubDirectory.HasPrevNext(sub)) {
                        m_PropertyScreen.OnSelectContinue = () => SetSubscreen(m_SubDirectory.GetNext(sub));
                        m_PropertyScreen.OnSelectBack = () => SetSubscreen(m_SubDirectory.GetPrevious(sub), true);
                    }
                }
            }
        }

        public void SetSubSequence(TankType Tank) {
            var sequence = Services.Tweaks.Get<ExperimentSettings>().GetTank(Tank).Sequence;
            if (m_SubDirectory.GetSequence() == null || m_SubDirectory.GetSequence() != sequence)
            {
                m_SubDirectory.SetSequence(sequence);
            }


            SetActions(sequence);
        }

        #endregion // Subscreens
    

        #region Routines


        private IEnumerator LoadFoundationalTankExperiment() {
            using(var tempFader = Services.UI.ScreenFaders.AllocFader())
            {
                Services.UI.ShowLetterbox();
                yield return tempFader.Object.Show(Color.black, 0.5f);
                Services.Events.Dispatch(ExperimentEvents.SetupInitialSubmit, m_SelectionData);
                m_ExperimentSetup = true;
                yield return 0.2f;
                Services.UI.HideLetterbox();
                SetSubscreen(m_SubDirectory.GetNext(ExpSubscreen.Begin));
                yield return tempFader.Object.Hide(0.5f, false);
                SetInputState(true);
            }
        }

        private IEnumerator StartFoundationalTankExperimentRoutine()
        {
            while(ExperimentServices.Actors.AnyActorsAreAnimating())
                yield return null;
            
            Hide();
            yield return 0.25f;
            Services.Events.Dispatch(ExperimentEvents.ExperimentBegin);
        }

        private IEnumerator ExitFoundationalTankExperimentRoutine()
        {
            using(var tempFader = Services.UI.ScreenFaders.AllocFader())
            {
                bool bWasRunning = m_ExperimentRunning;
                Services.UI.ShowLetterbox();
                yield return tempFader.Object.Show(Color.black, 0.5f);
                if (bWasRunning)
                {
                    ExperimentResultData result = new ExperimentResultData();
                    result.Setup = m_SelectionData.Clone();
                    Services.Events.Dispatch(ExperimentEvents.ExperimentRequestSummary, result);
                    m_SummaryScreen.Populate(result);
                }
                Services.Events.Dispatch(ExperimentEvents.ExperimentTeardown);
                yield return 0.2f;
                Services.UI.HideLetterbox();
                if (!bWasRunning)
                {
                    InstantHide();
                }
                else
                {
                    SetInputState(true);
                    SetSubscreen(m_SubDirectory.GetNext(ExpSubscreen.InProgress));
                }
                yield return tempFader.Object.Hide(0.5f, false);
            }
        }

        private IEnumerator StartStressorTankExperimentRoutine() {
            Hide();
            yield return 0.25f;
            Services.Events.Dispatch(ExperimentEvents.ExperimentBegin);
            yield return 3f;
        }

        #endregion // Routines

    }

}