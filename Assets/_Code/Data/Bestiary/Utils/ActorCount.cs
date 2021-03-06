using System;
using System.Collections.Generic;
using BeauUtil;
using UnityEngine;

namespace Aqua
{
    [Serializable]
    public struct ActorCountU32 : IKeyValuePair<StringHash32, uint>
    {
        public SerializedHash32 Id;
        public uint Population;

        public ActorCountU32(StringHash32 inId, uint inPopulation)
        {
            Id = inId;
            Population = inPopulation;
        }

        public StringHash32 Key { get { return Id; } }
        public uint Value { get { return Population; } }
    }

    [Serializable]
    public struct ActorCountI32 : IKeyValuePair<StringHash32, int>
    {
        public SerializedHash32 Id;
        public int Population;

        public ActorCountI32(StringHash32 inId, int inPopulation)
        {
            Id = inId;
            Population = inPopulation;
        }

        public StringHash32 Key { get { return Id; } }
        public int Value { get { return Population; } }
    }
}