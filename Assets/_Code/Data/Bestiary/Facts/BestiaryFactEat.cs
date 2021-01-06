using System.Collections.Generic;
using BeauPools;
using BeauUtil;
using UnityEngine;

namespace Aqua
{
    [CreateAssetMenu(menuName = "Aqualab/Bestiary/Fact/Eats")]
    public class BestiaryFactEat : BestiaryFactBase
    {
        #region Inspector

        [SerializeField] private BestiaryDesc m_SubjectEntry = null;
        [SerializeField] private BestiaryDesc m_TargetEntry = null;

        #endregion // Inspector

        public override void Accept(IFactVisitor inVisitor, PlayerFactParams inParams = null)
        {
            inVisitor.Visit(this, inParams);
        }

        public override IEnumerable<BestiaryFactFragment> GenerateFragments(PlayerFactParams inParams = null)
        {
            // TODO: Refine

            yield return BestiaryFactFragment.CreateSubjectVariant();
            yield return BestiaryFactFragment.CreateNoun(m_SubjectEntry.CommonName());
            
            // TODO: Generate the correct verb for the language
            
            yield return BestiaryFactFragment.CreateVerb("Eats");

            yield return BestiaryFactFragment.CreateTargetVariant();
            yield return BestiaryFactFragment.CreateNoun(m_TargetEntry.CommonName());
        }

        public override string GenerateSentence(PlayerFactParams inParams = null)
        {
            // TODO: localization!!!

            using(var psb = PooledStringBuilder.Create())
            {
                // TODO: Variants

                psb.Builder.Append(Services.Loc.MaybeLocalize(m_SubjectEntry.CommonName()))
                    .Append(" eats ")
                    .Append(Services.Loc.MaybeLocalize(m_TargetEntry.CommonName()));

                return psb.Builder.Flush();
            }
        }
    }
}