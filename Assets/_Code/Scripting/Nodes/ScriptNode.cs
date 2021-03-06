using System;
using BeauUtil.Blocks;
using System.Collections.Generic;
using UnityEngine.Scripting;
using BeauUtil;
using BeauPools;
using BeauUtil.Variants;
using UnityEngine;
using Leaf;
using BeauUtil.Debugger;

namespace Aqua.Scripting
{
    internal class ScriptNode : LeafNode
    {
        #region Serialized

        // Properties
        private string m_FullName = null;
        private ScriptNodeFlags m_Flags = 0;
        private ScriptNodePackage m_Package = null;
        private TriggerNodeData m_TriggerData = null;
        private StringHash32 m_TriggerOrFunctionId = null;
        private StringHash32 m_Target = null;
        private HashSet<StringHash32> m_Tags = new HashSet<StringHash32>();

        #endregion // Serialized

        public ScriptNode(ScriptNodePackage inPackage, string inFullId)
            : base(inFullId, inPackage)
        {
            m_Package = inPackage;
            m_Id = inFullId;
            m_FullName = inFullId;
        }

        public string FullName() { return m_FullName; }

        public ScriptNodeFlags Flags() { return m_Flags; }
        public ScriptNodePackage Package() { return m_Package; }

        public bool IsCutscene() { return (m_Flags & ScriptNodeFlags.Cutscene) != 0; }
        public bool IsTrigger() { return (m_Flags & ScriptNodeFlags.TriggerResponse) != 0; }
        public bool IsFunction() { return (m_Flags & ScriptNodeFlags.Function) != 0; }

        public TriggerNodeData TriggerData { get { return m_TriggerData; } }
        public IReadOnlyCollection<StringHash32> Tags() { return m_Tags; }
        public StringHash32 TriggerOrFunctionId() { return m_TriggerOrFunctionId; }

        public PersistenceLevel TrackingLevel()
        {
            if (m_TriggerData != null)
            {
                if (m_TriggerData.OnceLevel != PersistenceLevel.Untracked)
                    return m_TriggerData.OnceLevel;
                if (m_TriggerData.RepeatDuration > 0)
                    return PersistenceLevel.Session;
            }

            return PersistenceLevel.Untracked;
        }

        public StringHash32 TargetId()
        {
            return m_Target;
        }

        public TriggerPriority Priority()
        {
            if ((m_Flags & ScriptNodeFlags.Cutscene) != 0)
                return TriggerPriority.Cutscene;

            if (m_TriggerData != null)
            {
                return m_TriggerData.TriggerPriority;
            }

            return TriggerPriority.Low;
        }

        #region Parser

        [BlockMeta("cutscene"), Preserve]
        private void SetCutscene()
        {
            m_Flags |= ScriptNodeFlags.Cutscene;
        }

        [BlockMeta("ignoreDuringCutscene"), Preserve]
        private void IgnoreDuringCutscene()
        {
            m_Flags |= ScriptNodeFlags.SuppressDuringCutscene;
        }

        [BlockMeta("chatter"), Preserve]
        private void SetChatter()
        {
            m_Flags |= ScriptNodeFlags.CornerChatter | ScriptNodeFlags.SuppressDuringCutscene;
        }

        [BlockMeta("entrypoint"), Preserve]
        private void SetEntrypoint()
        {
            m_Flags |= ScriptNodeFlags.Entrypoint;
        }

        [BlockMeta("autosave"), Preserve]
        private void SetAutosave()
        {
            m_Flags |= ScriptNodeFlags.Autosave;
        }

        [BlockMeta("trigger"), Preserve]
        private void SetTriggerResponse(StringHash32 inTriggerId)
        {
            m_Flags |= ScriptNodeFlags.TriggerResponse;
            if (m_TriggerData == null)
            {
                m_TriggerData = new TriggerNodeData();
            }
            m_TriggerOrFunctionId = inTriggerId;

            // Mapping Shortcut - Partner requests are always towards kevin
            if (inTriggerId == GameTriggers.RequestPartnerHelp || inTriggerId == GameTriggers.PartnerTalk)
            {
                m_Target = GameConsts.Target_Kevin;;
            }
        }

        [BlockMeta("function"), Preserve]
        private void SetFunction(StringHash32 inFunctionId)
        {
            m_TriggerOrFunctionId = inFunctionId;
            m_Flags |= ScriptNodeFlags.Function;
        }

        [BlockMeta("who"), Preserve]
        private void SetTriggerTarget(StringHash32 inTargetId)
        {
            m_Target = inTargetId;
        }

        [BlockMeta("triggerPriority"), Preserve]
        private void SetTriggerPriority(TriggerPriority inPriority)
        {
            if (m_TriggerData != null)
            {
                m_TriggerData.TriggerPriority = inPriority;
            }
        }

        [BlockMeta("when"), Preserve]
        private void SetTriggerConditions(StringSlice inConditionsList)
        {
            if (m_TriggerData != null)
            {
                using(PooledList<StringSlice> conditions = PooledList<StringSlice>.Create())
                {
                    int conditionsCount = inConditionsList.Split(Parsing.QuoteAwareArgSplitter, StringSplitOptions.RemoveEmptyEntries, conditions);
                    if (conditionsCount > 0)
                    {
                        m_TriggerData.Conditions = new VariantComparison[conditionsCount];
                        for(int i = 0; i < conditionsCount; ++i)
                        {
                            if (!VariantComparison.TryParse(conditions[i], out m_TriggerData.Conditions[i]))
                            {
                                Log.Error("[ScriptNode] Unable to parse condition '{0}'", conditions[i]);
                            }
                        }

                        m_TriggerData.Score += conditionsCount;
                    }
                }
            }
        }

        [BlockMeta("boostScore"), Preserve]
        private void AdjustTriggerScore(int inScore)
        {
            if (m_TriggerData != null)
            {
                m_TriggerData.Score += inScore;
            }
        }

        [BlockMeta("once"), Preserve]
        private void SetOnce(StringSlice inCategory)
        {
            if (m_TriggerData != null)
            {
                m_TriggerData.RepeatDuration = 0;
                m_TriggerData.OnceLevel = inCategory.Equals("session") ? PersistenceLevel.Session : PersistenceLevel.Profile;
            }
        }

        [BlockMeta("repeat"), Preserve]
        private void SetRepeat(uint inDuration)
        {
            if (m_TriggerData != null)
            {
                m_TriggerData.RepeatDuration = (int) inDuration;
                m_TriggerData.OnceLevel = PersistenceLevel.Untracked;
            }
        }

        [BlockMeta("tags"), Preserve]
        private void SetTags(StringSlice inTags)
        {
            foreach(var tag in inTags.EnumeratedSplit(Parsing.CommaChar, StringSplitOptions.RemoveEmptyEntries))
                m_Tags.Add(tag.Trim());
        }

        #endregion // Parser
    }
    
    [Flags]
    internal enum ScriptNodeFlags
    {
        Cutscene = 0x01,
        Entrypoint = 0x02,
        TriggerResponse = 0x04,
        CornerChatter = 0x10,
        SuppressDuringCutscene = 0x20,
        Function = 0x40,
        Autosave = 0x80
    }
}