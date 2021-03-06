using System.Collections.Generic;
using Aqua;
using Aqua.Profile;
using BeauUtil;

namespace ProtoAqua.Modeling
{
    public class UniversalModelState
    {
        private readonly HashSet<StringHash32> m_GraphedCritters = new HashSet<StringHash32>();
        private readonly HashSet<StringHash32> m_GraphedFacts = new HashSet<StringHash32>();

        private readonly HashSet<StringHash32> m_UngraphedFacts = new HashSet<StringHash32>();

        public void Clear()
        {
            m_GraphedCritters.Clear();
            m_GraphedFacts.Clear();
        }

        public void Sync(BestiaryData inPlayerData)
        {
            m_GraphedCritters.Clear();
            m_GraphedCritters.Clear();
            m_UngraphedFacts.Clear();

            foreach(var graphedFactId in inPlayerData.GraphedFacts())
            {
                AddFact(Services.Assets.Bestiary.Fact(graphedFactId));
            }

            inPlayerData.GetUngraphedFacts(m_UngraphedFacts);
        }

        public bool IsFactGraphed(StringHash32 inFactId)
        {
            return m_GraphedFacts.Contains(inFactId);
        }

        public bool IsCritterGraphed(StringHash32 inCritterId)
        {
            return m_GraphedCritters.Contains(inCritterId);
        }

        public int UngraphedFactCount()
        {
            return m_UngraphedFacts.Count;
        }

        public void AddFact(BFBase inFact)
        {
            m_GraphedFacts.Add(inFact.Id());
            inFact.CollectReferences(m_GraphedCritters);
            m_UngraphedFacts.Remove(inFact.Id());
        }
    }
}