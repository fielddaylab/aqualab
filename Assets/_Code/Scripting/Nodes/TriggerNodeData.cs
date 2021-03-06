using System;
using BeauUtil.Blocks;
using System.Collections.Generic;
using UnityEngine.Scripting;
using BeauUtil;
using BeauUtil.Variants;

namespace Aqua.Scripting
{
    internal class TriggerNodeData
    {
        public TriggerPriority TriggerPriority;

        public VariantComparison[] Conditions;
        public int Score;

        public PersistenceLevel OnceLevel = PersistenceLevel.Untracked;
        public int RepeatDuration = 0;
    }
}