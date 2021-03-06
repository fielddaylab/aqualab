using UnityEngine;
using System;
using Aqua;
using UnityEngine.UI;
using AquaAudio;
using TMPro;
using BeauRoutine;
using BeauUtil;
using System.Collections.Generic;

namespace Aqua.Option
{
    public class ToggleOptionBar : MonoBehaviour 
    {
        #region Inspector

        [SerializeField] private LocText m_Label = null;
        [SerializeField] private CursorInteractionHint m_Hint = null;
        [SerializeField] private ToggleGroup m_ToggleGroup = null;
        [SerializeField] private LayoutGroup m_ToggleLayout = null;

        #endregion // Inspector

        [NonSerialized] private ToggleOptionBarItem[] m_Items;
        [NonSerialized] private int m_UsedItemCount;

        public CastableAction<object> OnChanged;

        private void InitItemList()
        {
            if (m_Items != null)
                return;

            m_Items = m_ToggleGroup.GetComponentsInChildren<ToggleOptionBarItem>(true);
            for(int i = 0; i < m_Items.Length; i++)
            {
                ToggleOptionBarItem item = m_Items[i];
                m_Items[i].Toggle.onValueChanged.AddListener((b) => { if (b) OnItemToggled(item.UserData); });
            }
        }

        public ToggleOptionBar Initialize<T>(TextId inLabel, TextId inDescription, Action<T> inSetter)
        {
            m_Label.SetText(inLabel);
            m_Hint.TooltipId = inDescription;
            OnChanged = CastableAction<object>.Create(inSetter);

            InitItemList();
            m_UsedItemCount = 0;
            return this;
        }

        public ToggleOptionBar AddOption(TextId inLabel, TextId inTooltip, object inValue)
        {
            ToggleOptionBarItem item = m_Items[m_UsedItemCount++];
            item.gameObject.SetActive(true);
            item.Initialize(inLabel, inTooltip, inValue);
            return this;
        }

        public void Build()
        {
            for(int i = m_UsedItemCount; i < m_Items.Length; i++)
            {
                m_Items[i].gameObject.SetActive(false);
            }

            m_ToggleLayout.ForceRebuild();
        }

        public void Sync<T>(T inValue)
        {
            ToggleOptionBarItem item = FindItem<T>(inValue);
            if (item != null)
                item.Toggle.SetIsOnWithoutNotify(true);
        }

        private ToggleOptionBarItem FindItem<T>(T inValue)
        {
            for(int i = 0; i < m_UsedItemCount; i++)
            {
                if (m_Items[i].UserData is T)
                {
                    T val = (T) m_Items[i].UserData;
                    if (EqualityComparer<T>.Default.Equals(inValue, val))
                    {
                        return m_Items[i];
                    }
                }
            }
            
            return null;
        }

        private void OnItemToggled(object inValue)
        {
            OnChanged.Invoke(inValue);

            OptionsData options = Services.Data.Options;
            options.SetDirty();
            
            Services.Events.QueueForDispatch(GameEvents.OptionsUpdated, options);
        }
    }
}