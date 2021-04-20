using BeauUtil;
using UnityEngine;
using System.Collections;
using BeauUtil.Debugger;
using Leaf;
using System;
using BeauUtil.Variants;
using Leaf.Runtime;
using Aqua.Portable;
using BeauRoutine;
using System.Collections.Generic;

namespace Aqua.Scripting
{
    public class ScriptDiveSiteProbe : ScriptComponent
    {
        private StringHash32 probeCritterId = null;
        private HashSet<StringHash32> ImproveFactIds = null;

        [LeafMember("AskForBestiaryEntry")]
        IEnumerator AskForBestiaryEntry()
        {
            Future<StringHash32> entity = BestiaryApp.RequestEntity(
                BestiaryDescCategory.Critter, (critterId) => EntryHasValues(critterId));
            entity.OnComplete(StoreId);
            entity.OnFail(() => Services.Data.SetVariable("temp:improveId", null));
            
            yield return entity.Wait();

        }

        [LeafMember("ImproveRules")]
        public void ImproveRules()
        {
            if(ImproveFactIds != null) {
                foreach(var factId in ImproveFactIds) {
                    Services.Data.Profile.Bestiary.GetFact(factId).Add(PlayerFactFlags.KnowValue);
                }
            }
            ImproveFactIds.Clear();

        }

        public bool EntryHasValues(BestiaryDesc critter) {
            foreach(var fact in Services.Data.Profile.Bestiary.GetFactsForEntity(critter.Id())) 
            {
                if(fact.Fact.HasValue() & !fact.Has(PlayerFactFlags.KnowValue)) {
                    ImproveFactIds.Add(fact.FactId);
                    return true;
                }
            }
            return false;
        }

        public void StoreId(StringHash32 critterId) {
            probeCritterId = critterId;
            var critter = Services.Assets.Bestiary.Get(critterId);
            Services.Data.SetVariable("temp:improveId", critter.CommonName());
        }
    }
}