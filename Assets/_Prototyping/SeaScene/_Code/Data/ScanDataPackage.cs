using System;
using UnityEngine;
using BeauData;
using BeauUtil;
using BeauUtil.Blocks;
using System.Collections.Generic;
using System.Collections;
using BeauUtil.Tags;
using System.Text;
using System.Threading;
using System.IO;

namespace ProtoAqua.Observation
{
    public class ScanDataPackage : IDataBlockPackage<ScanData>
    {
        private readonly Dictionary<StringHash, ScanData> m_Data = new Dictionary<StringHash, ScanData>(32);

        private string m_Name;
        [BlockMeta("basePath")] private string m_RootPath;

        public ScanDataPackage(string inName)
        {
            m_Name = inName;
            m_RootPath = inName;
        }

        public bool TryGetScanData(StringHash inId, out ScanData outData)
        {
            return m_Data.TryGetValue(inId, out outData);
        }

        #region ICollection

        public int Count { get { return m_Data.Count; } }

        public IEnumerator<ScanData> GetEnumerator()
        {
            return m_Data.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion // ICollection

        #region Generator

        public class Generator : AbstractBlockGenerator<ScanData, ScanDataPackage>
        {
            public override ScanDataPackage CreatePackage(string inFileName)
            {
                return new ScanDataPackage(inFileName);
            }

            public override bool TryCreateBlock(IBlockParserUtil inUtil, ScanDataPackage inPackage, TagData inId, out ScanData outBlock)
            {
                inUtil.TempBuilder.Length = 0;
                inUtil.TempBuilder.Append(inPackage.m_RootPath);
                if (!inPackage.m_RootPath.EndsWith("."))
                    inUtil.TempBuilder.Append('.');
                inUtil.TempBuilder.AppendSlice(inId.Id);
                string fullId = inUtil.TempBuilder.Flush();
                outBlock = new ScanData(fullId);
                inPackage.m_Data.Add(fullId, outBlock);
                return true;
            }
        }

        #endregion // Generator

        #if UNITY_EDITOR

        [UnityEditor.Experimental.AssetImporters.ScriptedImporter(1, "scan")]
        private class Importer : UnityEditor.Experimental.AssetImporters.ScriptedImporter
        {
            public override void OnImportAsset(UnityEditor.Experimental.AssetImporters.AssetImportContext ctx)
            {
                TextAsset txtAsset = new TextAsset(File.ReadAllText(ctx.assetPath));
                ctx.AddObjectToAsset("Main Object", txtAsset);
                ctx.SetMainObject(txtAsset);
            }
        }

        #endif // UNITY_EDITOR
    }
}