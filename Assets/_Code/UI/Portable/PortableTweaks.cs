using System;
using System.Collections.Generic;
using Aqua;
using BeauData;
using BeauUtil;
using UnityEngine;

namespace Aqua.Portable
{
    [CreateAssetMenu(menuName = "Aqualab/Portable/Portable Tweaks")]
    public class PortableTweaks : TweakAsset
    {        
        [Header("Colors")]
        [SerializeField] private Color m_CritterListColor = ColorBank.Orange;
        [SerializeField] private Color m_EcosystemListColor = ColorBank.Aqua;
        [SerializeField] private Color m_ModelListColor = ColorBank.Pink;

        protected override void Apply()
        {
            base.Apply();
        }

        public Color BestiaryListColor(BestiaryDescCategory inCategory)
        {
            switch(inCategory)
            {
                case BestiaryDescCategory.Critter:
                    return m_CritterListColor;
                case BestiaryDescCategory.Environment:
                    return m_EcosystemListColor;
                case BestiaryDescCategory.Model:
                    return m_ModelListColor;

                default:
                    throw new ArgumentOutOfRangeException("inCategory");
            }
        }
    }
}