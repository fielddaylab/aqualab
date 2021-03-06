using System;
using System.Collections.Generic;
using Aqua;
using Aqua.Debugging;
using BeauUtil;
using BeauUtil.Blocks;
using UnityEngine;

namespace ProtoAqua.Observation
{
    [CreateAssetMenu(menuName = "Aqualab/Observation/Scan Data Manager")]
    public class ScanDataMgr : TweakAsset
    {
        #region Types

        [Serializable]
        public class ScanTypeConfig
        {
            public Color BackgroundColor = ColorBank.White;
            public Color HeaderColor = ColorBank.Yellow;
            public Color TextColor = ColorBank.White;

            public string OpenSound;

            public ScanNodeConfig Node;
        }

        [Serializable]
        public class ScanNodeConfig
        {
            public Color UnscannedColor;
            public Color ScannedColor;
        }

        #endregion // Types

        #region Inspector

        [SerializeField] private ScanDataPackage[] m_DefaultAssets = null;
        [SerializeField] private ScanTypeConfig m_DefaultScanConfig = null;
        [SerializeField] private ScanTypeConfig m_ImportantScanConfig = null;
        [SerializeField] private float m_BaseScanDuration = 0.75f;
        [SerializeField] private float m_CompletedScanDuration = 0.2f;

        #endregion // Inspector

        private readonly HashSet<ScanDataPackage> m_Packages = new HashSet<ScanDataPackage>();
        private readonly Dictionary<StringHash32, ScanData> m_MasterMap = new Dictionary<StringHash32, ScanData>();

        public bool TryGetScanData(StringHash32 inId, out ScanData outData)
        {
            return m_MasterMap.TryGetValue(inId, out outData);
        }

        public bool WasScanned(StringHash32 inId) { return Services.Data.Profile.Inventory.WasScanned(inId); }

        public ScanResult RegisterScanned(ScanData inData)
        {
            if (Services.Data.Profile.Inventory.RegisterScanned(inData.Id()))
            {
                ScanResult result = ScanResult.NewScan;

                StringHash32 bestiaryId = inData.BestiaryId();
                if (!bestiaryId.IsEmpty && Services.Data.Profile.Bestiary.RegisterEntity(bestiaryId))
                {
                    result |= ScanResult.NewBestiary;
                }

                // TODO: Logbook

                foreach(var factId in inData.FactIds())
                {
                    if (Services.Data.Profile.Bestiary.RegisterFact(factId, false))
                    {
                        result |= ScanResult.NewBestiary;
                    }
                }
                
                return result;
            }

            return ScanResult.NoChange;
        }

        public ScanTypeConfig GetConfig(ScanDataFlags inFlags)
        {
            if ((inFlags & ScanDataFlags.Important) != 0)
                return m_ImportantScanConfig;

            return m_DefaultScanConfig;
        }

        public float GetScanDuration(ScanData inData)
        {
            if (inData == null)
                return m_BaseScanDuration;

            if (WasScanned(inData.Id()))
                return m_CompletedScanDuration;
            
            return m_BaseScanDuration * inData.ScanSpeed();
        }

        #region Register/Unregister

        public void Load(ScanDataPackage inPackage)
        {
            if (m_Packages.Add(inPackage))
            {
                inPackage.BindManager(this);
                inPackage.Parse(ScanDataPackage.Generator.Instance);
                AddPackage(inPackage);
            }
        }

        public void Unload(ScanDataPackage inPackage)
        {
            if (m_Packages.Remove(inPackage))
            {
                RemovePackage(inPackage);
                inPackage.BindManager(null);
                inPackage.Clear();
            }
        }

        internal void AddPackage(ScanDataPackage inPackage)
        {
            foreach(var node in inPackage)
            {
                m_MasterMap.Add(node.Id(), node);
            }

            DebugService.Log(LogMask.Observation | LogMask.Loading, "[ScanDataMgr] Loaded scan data package '{0}' with {1} nodes", inPackage.name, inPackage.Count);
        }

        internal void RemovePackage(ScanDataPackage inPackage)
        {
            foreach(var node in inPackage)
            {
                m_MasterMap.Remove(node.Id());
            }
            
            DebugService.Log(LogMask.Observation | LogMask.Loading, "[ScanDataMgr] Unloaded scan data package '{0}'", inPackage.name);
        }

        #endregion // Register/Unregister

        #region TweakAsset

        protected override void Apply()
        {
            foreach(var asset in m_DefaultAssets)
            {
                Load(asset);
            }
        }

        protected override void Remove()
        {
            foreach(var package in m_Packages)
            {
                package.BindManager(null);
                package.Clear();
            }
            m_Packages.Clear();
            m_MasterMap.Clear();
        }

        #endregion // TweakAsset
    }

    [Flags]
    public enum ScanResult : byte
    {
        NoChange =  0x0,
        NewScan =   0x1,
        NewLogbook = 0x2,
        NewBestiary = 0x4
    }
}