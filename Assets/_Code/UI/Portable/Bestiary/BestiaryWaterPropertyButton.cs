﻿using UnityEngine;
using BeauRoutine;
using BeauUtil;
using UnityEngine.UI;
using TMPro;
using Aqua;
using System;
using BeauUtil.Debugger;
using BeauPools;

namespace Aqua.Portable
{
    public class BestiaryWaterPropertyButton : MonoBehaviour, IPoolAllocHandler
    {
        #region Inspector

        [SerializeField, Required] private Button m_Button = null;
        [SerializeField, Required] private RectTransform m_ButtonTail = null;
        [SerializeField, Required] private Image m_Icon = null;
        [SerializeField, Required] private LocText m_Label = null;
        [SerializeField, Required] private LocText m_Value = null;
        [SerializeField, Required] private RectTransform m_SafeRangeTransform = null;

        #endregion // Inspector

        private PlayerFactParams m_Params;
        private Action<PlayerFactParams> m_Callback;

        public void Initialize(BFWaterProperty inFact, PlayerFactParams inParams, bool inbButtonMode, bool inbInteractable, Action<PlayerFactParams> inCallback)
        {
            var propData = Services.Assets.WaterProp.Property(inFact.PropertyId());

            m_Icon.sprite = inFact.Icon();
            m_Icon.gameObject.SetActive(inFact.Icon());

            m_Label.SetText(propData.LabelId());
            m_Value.SetText(propData.FormatValue(inFact.Value()));

            m_SafeRangeTransform.anchorMax = new Vector2(propData.RemapValue(inFact.Value()), 1f);

            m_Button.targetGraphic.raycastTarget = inbButtonMode;
            m_Button.interactable = inbInteractable;
            m_ButtonTail.gameObject.SetActive(inbButtonMode);

            m_Params = inParams ?? new PlayerFactParams(inFact.Id());
            m_Callback = inCallback;
        }

        private void OnClick()
        {
            Assert.NotNull(m_Callback);
            Assert.NotNull(m_Params);

            m_Callback(m_Params);
        }

        private void Awake()
        {
            m_Button.onClick.AddListener(OnClick);
        }

        void IPoolAllocHandler.OnAlloc()
        {
        }

        void IPoolAllocHandler.OnFree()
        {
            
            m_Params = null;
            m_Callback = null;
        }
    }
}