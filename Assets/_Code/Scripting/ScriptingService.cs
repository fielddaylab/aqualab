using System;
using System.Collections;
using System.Collections.Generic;
using BeauData;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Tags;
using UnityEngine;
using Aqua.Scripting;
using BeauUtil.Variants;
using Leaf.Runtime;
using Leaf;
using BeauUtil.Services;

namespace Aqua
{
    [ServiceDependency(typeof(DataService), typeof(UIMgr), typeof(LocService), typeof(AssetsService), typeof(TweakMgr))]
    public partial class ScriptingService : ServiceBehaviour
    {
        // thread management
        private Dictionary<string, ScriptThread> m_ThreadMap = new Dictionary<string, ScriptThread>(64, StringComparer.Ordinal);
        private Dictionary<StringHash32, ScriptThread> m_ThreadTargetMap = new Dictionary<StringHash32, ScriptThread>(8);
        private List<ScriptThread> m_ThreadList = new List<ScriptThread>(64);
        private ScriptThread m_CutsceneThread = null;
        
        // event parsing
        private TagStringEventHandler m_TagEventHandler;
        private CustomTagParserConfig m_TagEventParser;
        private StringUtils.ArgsList.Splitter m_ArgListSplitter;
        private LeafRuntime<ScriptNode> m_ThreadRuntime;
        private HashSet<StringHash32> m_SkippedEvents;
        private MethodCache<LeafMember> m_LeafCache;

        // trigger eval
        private CustomVariantResolver m_CustomResolver;

        // script nodes
        private HashSet<ScriptNodePackage> m_LoadedPackages;
        private Dictionary<LeafAsset, ScriptNodePackage> m_LoadedPackageSourcesAssets;

        private Dictionary<StringHash32, ScriptNode> m_LoadedEntrypoints;
        private Dictionary<StringHash32, TriggerResponseSet> m_LoadedResponses;
        private Dictionary<StringHash32, FunctionSet> m_LoadedFunctions;

        // objects
        [NonSerialized] private List<ScriptObject> m_ScriptObjects = new List<ScriptObject>();
        [NonSerialized] private bool m_ScriptObjectListDirty = false;

        // pools
        private IPool<VariantTable> m_TablePool;
        private IPool<ScriptThread> m_ThreadPool;
        private IPool<TagStringParser> m_ParserPool;

        // cached refs
        [NonSerialized] private ScriptingTweaks m_CachedTweaks;

        #region Refs

        public ScriptingTweaks Tweaks
        {
            get
            {
                if (m_CachedTweaks.IsReferenceNull())
                    m_CachedTweaks = Services.Tweaks.Get<ScriptingTweaks>();
                return m_CachedTweaks;
            }
        }

        #endregion // Refs

        #region Checks

        /// <summary>
        /// Returns if a thread is executing on the given target.
        /// </summary>
        public bool IsTargetExecuting(StringHash32 inTarget)
        {
            return m_ThreadTargetMap.ContainsKey(inTarget);
        }

        /// <summary>
        /// Returns the thread executing for the given target.
        /// </summary>
        public ScriptThreadHandle GetTargetThread(StringHash32 inTarget)
        {
            ScriptThread thread;
            if (m_ThreadTargetMap.TryGetValue(inTarget, out thread))
            {
                return thread.GetHandle();
            }

            return default(ScriptThreadHandle);
        }

        /// <summary>
        /// Returns if a cutscene thread is executing.
        /// </summary>
        public bool IsCutscene()
        {
            return m_CutsceneThread != null;
        }

        /// <summary>
        /// Returns the current cutscene thread.
        /// </summary>
        public ScriptThreadHandle GetCutscene()
        {
            return m_CutsceneThread?.GetHandle() ?? default(ScriptThreadHandle);
        }

        #endregion // Checks

        #region Operations

        #region Starting Threads with IEnumerator

        /// <summary>
        /// Returns a new scripting thread running the given IEnumerator.
        /// </summary>
        public ScriptThreadHandle StartThread(IEnumerator inEnumerator)
        {
            return StartThreadInternal(null, null, inEnumerator);
        }

        /// <summary>
        /// Returns a new scripting thread with the given id running the given IEnumerator.
        /// </summary>
        public ScriptThreadHandle StartThread(IEnumerator inEnumerator, string inThreadId)
        {
            return StartThreadInternal(inThreadId, null, inEnumerator);
        }

        /// <summary>
        /// Returns a new scripting thread running the given IEnumerator and attached to the given context.
        /// </summary>
        public ScriptThreadHandle StartThread(IScriptContext inContext, IEnumerator inEnumerator)
        {
            return StartThreadInternal(null, inContext, inEnumerator);
        }

        /// <summary>
        /// Returns a new scripting thread running the given IEnumerator and attached to the given context.
        /// </summary>
        public ScriptThreadHandle StartThread(IScriptContext inContext, IEnumerator inEnumerator, string inThreadId)
        {
            return StartThreadInternal(inThreadId, inContext, inEnumerator);
        }

        #endregion // Starting Threads with IEnumerator

        #region Starting Threads with Entrypoint

        /// <summary>
        /// Returns a new scripting thread running the given ScriptNode entrypoint.
        /// </summary>
        public ScriptThreadHandle StartNode(StringHash32 inEntrypointId)
        {
            ScriptNode node;
            if (!TryGetEntrypoint(inEntrypointId, out node))
            {
                Debug.LogWarningFormat("[ScriptingService] No entrypoint '{0}' is currently loaded", inEntrypointId.ToDebugString());
                return default(ScriptThreadHandle);
            }

            return StartThreadInternalNode(null, null, node, null);
        }

        /// <summary>
        /// Returns a new scripting thread with the given id running the given ScriptNode entrypoint.
        /// </summary>
        public ScriptThreadHandle StartNode(StringHash32 inEntrypointId, string inThreadId)
        {
            ScriptNode node;
            if (!TryGetEntrypoint(inEntrypointId, out node))
            {
                Debug.LogWarningFormat("[ScriptingService] No entrypoint '{0}' is currently loaded", inEntrypointId.ToDebugString());
                return default(ScriptThreadHandle);
            }

            return StartThreadInternalNode(inThreadId, null, node, null);
        }

        /// <summary>
        /// Returns a new scripting thread running the given ScriptNode entrypoint and attached to the given context.
        /// </summary>
        public ScriptThreadHandle StartNode(IScriptContext inContext, StringHash32 inEntrypointId)
        {
            ScriptNode node;
            if (!TryGetEntrypoint(inEntrypointId, out node))
            {
                Debug.LogWarningFormat("[ScriptingService] No entrypoint '{0}' is currently loaded", inEntrypointId.ToDebugString());
                return default(ScriptThreadHandle);
            }

            return StartThreadInternalNode(null, inContext, node, null);
        }

        /// <summary>
        /// Returns a new scripting thread running the given ScriptNode entrypoint and attached to the given context.
        /// </summary>
        public ScriptThreadHandle StartNode(IScriptContext inContext, StringHash32 inEntrypointId, string inThreadId)
        {
            ScriptNode node;
            if (!TryGetEntrypoint(inEntrypointId, out node))
            {
                Debug.LogWarningFormat("[ScriptingService] No entrypoint '{0}' is currently loaded", inEntrypointId.ToDebugString());
                return default(ScriptThreadHandle);
            }

            return StartThreadInternalNode(inThreadId, inContext, node, null);
        }

        #endregion // Starting Threads with Entrypoint

        #region Triggering Responses

        /// <summary>
        /// Attempts to trigger a response.
        /// </summary>
        public ScriptThreadHandle TriggerResponse(StringHash32 inTriggerId, StringHash32 inTarget = default(StringHash32), IScriptContext inContext = null, VariantTable inContextTable = null, string inThreadId = null)
        {
            ScriptThreadHandle handle = default(ScriptThreadHandle);
            IVariantResolver resolver = GetResolver(inContextTable);
            TriggerResponseSet responseSet;
            if (m_LoadedResponses.TryGetValue(inTriggerId, out responseSet))
            {
                using(PooledList<ScriptNode> nodes = PooledList<ScriptNode>.Create())
                {
                    int minScore = int.MinValue;
                    int responseCount = responseSet.GetHighestScoringNodes(resolver, inContext, Services.Data.Profile?.Script, inTarget, m_ThreadTargetMap, nodes, ref minScore);
                    if (responseCount > 0)
                    {
                        ScriptNode node = RNG.Instance.Choose(nodes);
                        Debug.LogFormat("[ScriptingService] Trigger '{0}' -> Running node '{1}'", inTriggerId.ToDebugString(), node.Id().ToDebugString());
                        handle = StartThreadInternalNode(inThreadId, inContext, node, inContextTable);
                    }
                }
            }
            if (!handle.IsRunning())
            {
                Debug.LogFormat("[ScriptingService] Trigger '{0}' had no valid responses", inTriggerId.ToDebugString());
            }
            ResetCustomResolver();
            return handle;
        }

        private IVariantResolver GetResolver(VariantTable inContext)
        {
            if (inContext == null || (inContext.Count == 0 && inContext.Base == null))
                return Services.Data.VariableResolver;
            
            if (m_CustomResolver == null)
            {
                m_CustomResolver = new CustomVariantResolver();
                m_CustomResolver.Base = Services.Data.VariableResolver;
            }

            m_CustomResolver.SetDefaultTable(inContext);
            return m_CustomResolver;
        }

        private void ResetCustomResolver()
        {
            if (m_CustomResolver != null)
            {
                m_CustomResolver.ClearDefaultTable();
            }
        }

        #endregion // Triggering Responses

        #region Killing Threads

        /// <summary>
        /// Kills a currently running scripting thread.
        /// </summary>
        public bool KillThread(string inThreadId)
        {
            ScriptThread thread;
            
            // wildcard id match
            if (inThreadId.IndexOf('*') >= 0)
            {
                bool bKilled = false;
                for(int i = m_ThreadList.Count - 1; i >= 0; --i)
                {
                    thread = m_ThreadList[i];
                    string id = thread.Name;
                    if (StringUtils.WildcardMatch(id, inThreadId))
                    {
                        thread.Kill();
                        bKilled = true;
                    }
                }

                return bKilled;
            }
            else
            {
                if (m_ThreadMap.TryGetValue(inThreadId, out thread))
                {
                    thread.Kill();
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kills all currently running scripting threads for the given context.
        /// </summary>
        public bool KillThreads(IScriptContext inContext)
        {
            bool bKilled = false;
            ScriptThread thread;
            for(int i = m_ThreadList.Count - 1; i >= 0; --i)
            {
                thread = m_ThreadList[i];
                if (thread.Context == inContext)
                {
                    string id = thread.Name;
                    thread.Kill();
                    bKilled = true;
                }
            }
            return bKilled;
        }

        /// <summary>
        /// Kills all currently running threads.
        /// </summary>
        public void KillAllThreads()
        {
            for(int i = m_ThreadList.Count - 1; i >= 0; --i)
            {
                m_ThreadList[i].Kill();
            }

            m_ThreadList.Clear();
            m_ThreadMap.Clear();
            m_ThreadTargetMap.Clear();
            m_CutsceneThread = null;
        }

        /// <summary>
        /// Kills all threads with a priority less than the given priority
        /// </summary>
        public void KillLowPriorityThreads(TriggerPriority inThreshold = TriggerPriority.Cutscene)
        {
            for(int i = m_ThreadList.Count - 1; i >= 0; --i)
            {
                var thread = m_ThreadList[i];
                if (thread.Priority() < inThreshold)
                    thread.Kill();
            }
        }

        /// <summary>
        /// Kills all threads for the given target.
        /// </summary>
        public bool KillTargetThread(StringHash32 inTargetId)
        {
            ScriptThread thread;
            if (m_ThreadTargetMap.TryGetValue(inTargetId, out thread))
            {
                thread.Kill();
                return true;
            }

            return false;
        }

        #endregion // Killing Threads

        #region Calling Methods

        /// <summary>
        /// Executes a script command.
        /// Format should be "method arg0, arg1, arg2, ..." or "targetId->method arg0, arg1, arg2, ..."
        /// </summary>
        public object Execute(StringSlice inCommand)
        {
            StringSlice target = StringSlice.Empty, method, args;
            var methodArgs = TagData.Parse(inCommand, Parsing.InlineEvent);
            method = methodArgs.Id;
            args = methodArgs.Data;

            int indirectIndex = method.IndexOf("->");
            if (indirectIndex >= 0)
            {
                target = method.Substring(0, indirectIndex);
                method = method.Substring(indirectIndex + 2);
            }

            object result;
            if (target.IsEmpty)
            {
                m_LeafCache.TryStaticInvoke(method, args, out result);
            }
            else
            {
                ScriptObject targetObj;
                if (!TryGetScriptObjectById(target, out targetObj))
                {
                    Debug.LogWarningFormat("[ScriptingService] No ScriptObject with id '{0}' exists");
                    result = null;
                }
                else
                {
                    m_LeafCache.TryInvoke(targetObj, method, args, out result);
                }
            }

            return result;
        }

        #endregion // Calling Methods

        #endregion // Operations

        #region Contexts

        public TempVarTable GetTempTable()
        {
            var table = m_TablePool.TempAlloc();
            table.Object.Name = "temp";
            return new TempVarTable(table);
        }

        public TempVarTable GetTempTable(VariantTable inBase)
        {
            var table = m_TablePool.TempAlloc();
            table.Object.Name = "temp";
            table.Object.Base = inBase;
            return new TempVarTable(table);
        }

        #endregion // Contexts

        #region Utils

        /// <summary>
        /// Parses a string into a TagString.
        /// </summary>
        public TagString ParseToTag(StringSlice inLine, object inContext = null)
        {
            TagString str = new TagString();
            ParseToTag(ref str, inLine, inContext);
            return str;
        }

        /// <summary>
        /// Parses a string into a TagString.
        /// </summary>
        public void ParseToTag(ref TagString ioTag, StringSlice inLine, object inContext = null)
        {
            TagStringParser parser = m_ParserPool.Alloc();
            parser.Parse(ref ioTag, inLine, inContext);
            m_ParserPool.Free(parser);
        }

        #endregion // Utils

        #region Internal

        internal void UntrackThread(ScriptThread inThread)
        {
            m_ThreadList.FastRemove(inThread);

            string name = inThread.Name;
            if (!string.IsNullOrEmpty(name))
                m_ThreadMap.Remove(name);

            StringHash32 who = inThread.Target();
            if (!who.IsEmpty)
                m_ThreadTargetMap.Remove(who);

            if (m_CutsceneThread == inThread)
                m_CutsceneThread = null;
        }

        // Starts a scripting thread
        private ScriptThreadHandle StartThreadInternal(string inThreadName, IScriptContext inContext, IEnumerator inEnumerator)
        {
            if (inEnumerator == null || !FreeName(inThreadName))
            {
                return default(ScriptThreadHandle);
            }

            ScriptThread thread = m_ThreadPool.Alloc();
            ScriptThreadHandle handle = thread.Prep(inThreadName, inContext, null);
            thread.AttachToRoutine(Routine.Start(this, inEnumerator));

            m_ThreadList.Add(thread);
            if (!string.IsNullOrEmpty(inThreadName))
                m_ThreadMap.Add(inThreadName, thread);
            return handle;
        }

        private ScriptThreadHandle StartThreadInternalNode(string inThreadName, IScriptContext inContext, ScriptNode inNode, VariantTable inVars)
        {
            if (inNode == null || !FreeName(inThreadName) || !CheckPriority(inNode))
            {
                return default(ScriptThreadHandle);
            }

            if (inNode.IsCutscene())
            {
                m_CutsceneThread?.Kill();
            }

            TempAlloc<VariantTable> tempVars = m_TablePool.TempAlloc();
            if (inVars != null && inVars.Count > 0)
            {
                inVars.CopyTo(tempVars.Object);
                tempVars.Object.Base = inVars.Base;
            }

            ScriptThread thread = m_ThreadPool.Alloc();
            ScriptThreadHandle handle = thread.Prep(inThreadName, inContext, tempVars);
            thread.SyncPriority(inNode);
            thread.AttachToRoutine(Routine.Start(this, ProcessNodeInstructions(thread, inNode)));

            m_ThreadList.Add(thread);
            if (!string.IsNullOrEmpty(inThreadName))
                m_ThreadMap.Add(inThreadName, thread);

            StringHash32 who = thread.Target();
            if (!who.IsEmpty)
                m_ThreadTargetMap.Add(who, thread);

            if (inNode.IsCutscene())
            {
                m_CutsceneThread = thread;
            }
            
            return handle;
        }

        private bool FreeName(string inThreadName)
        {
            bool bHasId = !string.IsNullOrEmpty(inThreadName);
            if (bHasId)
            {
                if (inThreadName.IndexOf('*') >= 0)
                {
                    Debug.LogErrorFormat("[ScriptingService] Thread id of '{0}' is invalid - contains wildchar", inThreadName);
                    return false;
                }

                ScriptThread current;
                if (m_ThreadMap.TryGetValue(inThreadName, out current))
                {
                    current.Kill();
                }
            }

            return true;
        }

        private bool CheckPriority(ScriptNode inNode)
        {
            StringHash32 target = inNode.TargetId();
            if (target.IsEmpty)
                return true;

            ScriptThread thread;
            if (m_ThreadTargetMap.TryGetValue(target, out thread))
            {
                if (thread.Priority() >= inNode.Priority())
                {
                    Debug.LogFormat("[ScriptingService] Could not trigger node '{0}' on target '{1}' - higher priority thread already running for given target",
                        inNode.Id().ToDebugString(), target.ToDebugString());
                    return false;
                }

                Debug.LogFormat("[ScriptingService] Killed thread with priority '{0}' running on target '{1}' - higher priority node '{2}' was requested",
                    thread.Priority(), target.ToDebugString(), inNode.Id().ToDebugString());

                thread.Kill();
                m_ThreadTargetMap.Remove(target);
            }

            return true;
        }

        #endregion // Internal

        #region IService

        protected override void Initialize()
        {
            InitParsers();
            InitHandlers();

            m_LeafCache = new MethodCache<LeafMember>();
            m_LeafCache.LoadStatic();

            m_ParserPool = new DynamicPool<TagStringParser>(4, (p) => {
                var parser = new TagStringParser();
                parser.Delimiters = Parsing.InlineEvent;
                parser.EventProcessor = m_TagEventParser;
                parser.ReplaceProcessor = m_TagEventParser;
                return parser;
            });

            m_ThreadRuntime = new LeafRuntime<ScriptNode>(this);

            m_LoadedPackages = new HashSet<ScriptNodePackage>();
            m_LoadedEntrypoints = new Dictionary<StringHash32, ScriptNode>(256);
            m_LoadedResponses = new Dictionary<StringHash32, TriggerResponseSet>();
            m_LoadedPackageSourcesAssets = new Dictionary<LeafAsset, ScriptNodePackage>();

            m_TablePool = new DynamicPool<VariantTable>(8, Pool.DefaultConstructor<VariantTable>());
            m_TablePool.Config.RegisterOnFree((p, obj) => { obj.Reset(); });

            m_ThreadPool = new DynamicPool<ScriptThread>(16, Pool.DefaultConstructor<ScriptThread>());
            m_ThreadPool.Config.RegisterOnConstruct((p, obj) => { obj.Initialize(this, Services.Data.VariableResolver); });
        }

        protected override void Shutdown()
        {
            m_TagEventParser = null;
            m_TagEventHandler = null;

            m_TablePool.Dispose();
            m_TablePool = null;

            m_ScriptObjects.Clear();
        }

        #endregion // IService
    }
}