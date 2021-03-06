using UnityEngine;
using UnityEngine.UI;
using BeauRoutine;
using BeauRoutine.Extensions;
using BeauUtil;
using System;
using Aqua.Scripting;

namespace Aqua.Portable
{
    public abstract class PortableMenuApp : BasePanel
    {
        #region Inspector

        [Header("Portable App")]
        [SerializeField] private SerializedHash32 m_Id = null;

        #endregion // Inspector

        [NonSerialized] protected PortableMenu m_ParentMenu;

        public StringHash32 Id() { return m_Id; }

        public virtual bool TryHandle(IPortableRequest inRequest)
        {
            return false;
        }

        protected override void Awake()
        {
            base.Awake();

            m_ParentMenu = GetComponentInParent<PortableMenu>();
        }

        protected override void OnShow(bool inbInstant)
        {
            base.OnShow(inbInstant);

            Services.Data.SetVariable("portable:app", m_Id.Hash());
        }

        protected override void OnShowComplete(bool inbInstant)
        {
            base.OnShowComplete(inbInstant);

            using(var table = TempVarTable.Alloc())
            {
                table.Set("appId", m_Id.Hash());
                Services.Script.TriggerResponse(GameTriggers.PortableAppOpened, null, null, table);
            }
        }

        protected override void OnHide(bool inbInstant)
        {
            if (Services.Data)
            {
                Services.Data.CompareExchange("portable:app", m_Id.Hash(), null);
            }

            base.OnHide(inbInstant);
        }
    }
}