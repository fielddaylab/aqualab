﻿using System.Collections.Generic;
using Aqua;
using BeauUtil.Blocks;
using UnityEngine;

namespace ProtoAqua.Argumentation
{
    [CreateAssetMenu(menuName = "Aqualab/Argumentation/Graph Data Manager")]
    public class GraphDataManager : TweakAsset
    {
        [SerializeField] private GraphDataPackage[] m_DefaultAssets = null;

        private Dictionary<string, GraphDataPackage> m_Packages = new Dictionary<string, GraphDataPackage>();

        private GraphDataPackage.Generator m_Generator = new GraphDataPackage.Generator();

        public GraphDataPackage GetPackage(string name)
        {
            if (m_Packages.TryGetValue(name, out GraphDataPackage package))
            {
                return package;
            }

            throw new System.ArgumentNullException($"No package '{name}' was found");
        }

        #region TweakAsset

        protected override void Apply()
        {
            if (m_Packages.Count >= m_DefaultAssets.Length)
                return;
            
            foreach (var asset in m_DefaultAssets)
            {
                asset.Parse(m_Generator);
                m_Packages.Add(asset.name, asset);
            }
        }

        protected override void Remove()
        {
        }

        #endregion // TweakAsset
    }
}
