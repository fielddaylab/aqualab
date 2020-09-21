using System;
using System.Collections;
using System.Collections.Generic;
using BeauData;
using BeauPools;
using BeauRoutine;
using BeauUtil;
using BeauUtil.Tags;
using UnityEngine;
using UnityEngine.SceneManagement;
using BeauUtil.Debugger;
using BeauUtil.Variants;

namespace ProtoAqua
{
    public partial class StateMgr : ServiceBehaviour
    {
        private Routine m_SceneLoadRoutine;
        private bool m_SceneLock;

        private VariantTable m_TempSceneTable;

        #region Scene Loading

        /// <summary>
        /// Loads to another scene.
        /// </summary>
        public IEnumerator LoadScene(string inSceneName, object inContext = null, bool inbAutoHideLoading = true)
        {
            if (m_SceneLock)
            {
                Debug.LogErrorFormat("[StateMgr] Scene load already in progress");
                return null;
            }

            SceneBinding scene = SceneHelper.FindSceneByName(inSceneName, SceneCategories.Build);
            if (!scene.IsValid())
            {
                Debug.LogErrorFormat("[StateMgr] No scene found with name matching '{0}'", inSceneName);
                return null;
            }

            m_SceneLock = true;
            m_SceneLoadRoutine.Replace(this, SceneSwap(scene, inContext, inbAutoHideLoading)).TryManuallyUpdate(0);
            return m_SceneLoadRoutine.Wait();
        }

        /// <summary>
        /// Loads to another scene.
        /// </summary>
        public IEnumerator LoadScene(SceneBinding inScene, object inContext = null, bool inbAutoHideLoading = true)
        {
            if (m_SceneLock)
            {
                Debug.LogErrorFormat("[StateMgr] Scene load already in progress");
                return null;
            }

            if (!inScene.IsValid())
            {
                Debug.LogErrorFormat("[StateMgr] Provided scene '{0}' is not valid", inScene);
                return null;
            }

            m_SceneLock = true;
            m_SceneLoadRoutine.Replace(this, SceneSwap(inScene, inContext, inbAutoHideLoading)).TryManuallyUpdate(0);
            return m_SceneLoadRoutine.Wait();
        }

        /// <summary>
        /// Loads to another scene.
        /// </summary>
        public IEnumerator LoadScene(StringHash inSceneId, object inContext = null, bool inbAutoHideLoading = true)
        {
            if (m_SceneLock)
            {
                Debug.LogErrorFormat("[StateMgr] Scene load already in progress");
                return null;
            }

            SceneBinding scene = SceneHelper.FindSceneById(inSceneId, SceneCategories.Build);
            if (!scene.IsValid())
            {
                Debug.LogErrorFormat("[StateMgr] No scene found with id '{0}'", inSceneId.ToString());
                return null;
            }

            m_SceneLock = true;
            m_SceneLoadRoutine.Replace(this, SceneSwap(scene, inContext, inbAutoHideLoading)).TryManuallyUpdate(0);
            return m_SceneLoadRoutine.Wait();
        }

        /// <summary>
        /// Returns if loading into another scene.
        /// </summary>
        public bool IsLoadingScene()
        {
            return m_SceneLock;
        }

        private IEnumerator InitialSceneLoad()
        {
            yield return WaitForLoadingAndCleanup();

            m_SceneLock = false;
            Services.UI.HideLoadingScreen();

            BindScene(SceneHelper.FindScene(SceneCategories.ActiveOnly));

            foreach(var scene in SceneHelper.FindScenes(SceneCategories.AllLoaded))
            {
                Debug.LogFormat("[StateMgr] Initial load of '{0}' finished", scene.Path);
                scene.BroadcastLoaded();
            }
        }

        private IEnumerator SceneSwap(SceneBinding inNextScene, object inContext, bool inbAutoHideLoading)
        {
            try
            {
                yield return Services.UI.ShowLoadingScreen();

                SceneBinding active = SceneHelper.FindScene(SceneCategories.ActiveOnly);
                Debug.LogFormat("[StateMgr] Unloading scene '{0}'", active.Path);
                active.BroadcastUnload(inContext);
                
                AsyncOperation loadOp = SceneManager.LoadSceneAsync(inNextScene.Path, LoadSceneMode.Single);
                loadOp.allowSceneActivation = false;

                Debug.LogFormat("[StateMgr] Loading scene '{0}'", inNextScene.Path);

                while(loadOp.progress < 0.9f)
                    yield return null;

                loadOp.allowSceneActivation = true;

                while(!loadOp.isDone)
                    yield return null;

                yield return WaitForLoadingAndCleanup();
                BindScene(inNextScene);

                m_SceneLock = false;

                if (inbAutoHideLoading)
                    Services.UI.HideLoadingScreen();

                Debug.LogFormat("[StateMgr] Finished loading scene '{0}'", inNextScene.Path);
                inNextScene.BroadcastLoaded(inContext);
            }
            finally
            {
                m_SceneLock = false;
            }
        }

        private IEnumerator WaitForLoadingAndCleanup()
        {
            while(BuildInfo.IsLoading())
                yield return null;

            foreach(var service in Services.All())
            {
                if (ReferenceEquals(this, service))
                    continue;
                
                while(service.IsLoading())
                    yield return null;
            }

            using(Profiling.Time("gc collect"))
            {
                GC.Collect();
            }
            using(Profiling.Time("unload unused assets"))
            {
                yield return Resources.UnloadUnusedAssets();
            }
        }

        #endregion // Scene Loading

        #region Scripting

        private void BindScene(SceneBinding inScene)
        {
            if (m_TempSceneTable == null)
            {
                m_TempSceneTable = new VariantTable("temp");
                m_TempSceneTable.Capacity = 64;

                Services.Data.BindTable("temp", m_TempSceneTable);
            }
            else
            {
                m_TempSceneTable.Clear();
            }
        }

        #endregion // Scripting

        #region IService

        protected override void OnRegisterService()
        {
            m_SceneLoadRoutine.Replace(this, InitialSceneLoad());
            m_SceneLock = true;
        }

        protected override void OnDeregisterService()
        {
            m_SceneLoadRoutine.Stop();
        }

        protected override bool IsLoading()
        {
            return m_SceneLoadRoutine && m_SceneLock;
        }

        public override FourCC ServiceId()
        {
            return ServiceIds.State;
        }

        #endregion // IService
    }
}