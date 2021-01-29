#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif

using System.Collections.Generic;
using System.Reflection;
using BeauData;
using BeauPools;
using BeauUtil;
using AquaAudio;
using UnityEngine;
using BeauUtil.Services;
using UnityEngine.SceneManagement;

namespace Aqua
{
    public class Services
    {
        #region Cache

        static Services()
        {
            Application.quitting += () => { s_Quitting = true; };
        }
        
        static private readonly ServiceCache s_ServiceCache = new ServiceCache();
        static private bool s_Quitting;

        static public bool Valid { get { return !s_Quitting; } }

        #endregion // Cache

        #region Accessors

        [ServiceReference] static public AnalyticsService Analytics { get; private set; }
        [ServiceReference] static public AssetsService Assets { get; private set; }
        [ServiceReference] static public AudioMgr Audio { get; private set; }
        [ServiceReference] static public DataService Data { get; private set; }
        [ServiceReference] static public EventService Events { get; private set; }
        [ServiceReference] static public InputService Input { get; private set; }
        [ServiceReference] static public LocService Loc { get; private set; }
        [ServiceReference] static public ScriptingService Script { get; private set; }
        [ServiceReference] static public StateMgr State { get; private set; }
        [ServiceReference] static public TweakMgr Tweaks { get; private set; }
        [ServiceReference] static public UIMgr UI { get; private set; }
    
        #endregion // Accessors

        #region Setup

        static public void AutoSetup(GameObject inRoot)
        {
            s_ServiceCache.AddFromHierarchy(inRoot.transform);
            s_ServiceCache.Process();
        }

        static public void AutoSetup(Scene inScene)
        {
            s_ServiceCache.AddFromScene(inScene);
            s_ServiceCache.Process();
        }

        static public void Deregister(IService inService)
        {
            s_ServiceCache.Remove(inService);
            s_ServiceCache.Process();
        }

        static public void Deregister(Scene inScene)
        {
            s_ServiceCache.RemoveFromScene(inScene);
            s_ServiceCache.Process();
        }

        static public void Shutdown()
        {
            s_ServiceCache.ClearAll();
        }

        #endregion // Setup

        #region All

        static public IEnumerable<IService> All()
        {
            return s_ServiceCache.All<IService>();
        }

        static public IEnumerable<ILoadable> AllLoadable()
        {
            return s_ServiceCache.All<ILoadable>();
        }

        #endregion // All
    }
}