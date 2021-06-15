using System;
using BeauUtil;
using UnityEngine;

namespace Aqua
{
    [RequireComponent(typeof(Light))]
    public class TimeAnimatedLight : TimeAnimatedObject
    {
        #region Inspector

        [SerializeField, Required(ComponentLookupDirection.Self)] public Light Light;
        [Inline(InlineAttribute.DisplayType.HeaderLabel)] public TimeColorPalette Palette;

        #endregion // Inspector

        public override void OnTimeChanged(InGameTime inGameTime)
        {
            Light.color = Palette.GetValueForTime(inGameTime);
        }
    }
}