using System;
using System.Collections.Generic;
using BeauUtil;
using UnityEngine;

namespace Aqua
{
    public struct ActorStateTransitionRange
    {
        public float AliveMin;
        public float AliveMax;
        public float StressedMin;
        public float StressedMax;

        public void Reset()
        {
            AliveMin = StressedMin = float.NegativeInfinity;
            AliveMax = StressedMax = float.PositiveInfinity;
        }

        public ActorStateId Evaluate(float inValue)
        {
            if (inValue < StressedMin || inValue > StressedMax)
                return ActorStateId.Dead;
            if (inValue < AliveMin || inValue > AliveMax)
                return ActorStateId.Stressed;
            return ActorStateId.Alive;
        }

        static ActorStateTransitionRange()
        {
            s_DefaultRange = new ActorStateTransitionRange();
            s_DefaultRange.Reset();
        }

        static private readonly ActorStateTransitionRange s_DefaultRange;

        static public ActorStateTransitionRange Default { get { return s_DefaultRange; } }
    }
}