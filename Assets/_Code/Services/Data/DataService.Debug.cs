#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define DEVELOPMENT
#endif // UNITY_EDITOR || DEVELOPMENT_BUILD

using Aqua.Debugging;
using Aqua.Profile;
using BeauData;
using BeauUtil;
using BeauUtil.Debugger;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace Aqua
{
    public partial class DataService : ServiceBehaviour, IDebuggable, ILoadable
    {
        #if UNITY_EDITOR
        [NonSerialized] private DMInfo m_BookmarksMenu;
        #endif // UNITY_EDITOR

        internal void UseDebugProfile()
        {
            ClearOldProfile();

            SaveData saveData = CreateNewProfile();
            DebugService.Log(LogMask.DataService, "[DataService] Created debug profile");
            DeclareProfile(saveData, string.Empty, false);
        }

        private void LoadBookmark(string inBookmarkName)
        {
            TextAsset bookmarkAsset = Resources.Load<TextAsset>("Bookmarks/" + inBookmarkName);
            if (!bookmarkAsset)
                return;

            SaveData bookmark;
            if (TryLoadProfileFromBytes(bookmarkAsset.bytes, out bookmark))
            {
                ClearOldProfile();

                DebugService.Log(LogMask.DataService, "[DataService] Loaded profile from bookmark '{0}'", inBookmarkName);

                DeclareProfile(bookmark, null, false);
                StartPlaying(null, true);
            }
        }

        private void ForceReloadSave()
        {
            LoadProfile(m_ProfileName).OnComplete((success) => {
                if (success)
                    StartPlaying(null, true);
            });
        }

        private void ForceRestart()
        {
            DeleteSave(m_ProfileName);
            NewProfile(m_ProfileName).OnComplete((success) => {
                if (success)
                    StartPlaying(null, true);
            });
        }

        private void DebugSaveData()
        {
            SyncProfile();

            #if UNITY_EDITOR

            Directory.CreateDirectory("Saves");
            string saveName = string.Format("Saves/save_{0}.json", m_CurrentSaveData.LastUpdated);
            string binarySaveName = string.Format("Saves/save_{0}.bbin", m_CurrentSaveData.LastUpdated);
            Serializer.WriteFile(m_CurrentSaveData, saveName, OutputOptions.PrettyPrint, Serializer.Format.JSON);
            Serializer.WriteFile(m_CurrentSaveData, binarySaveName, OutputOptions.None, Serializer.Format.Binary);
            Debug.LogFormat("[DataService] Saved Profile to {0} and {1}", saveName, binarySaveName);
            EditorUtility.OpenWithDefaultApp(saveName);

            #elif DEVELOPMENT_BUILD

            string json = Serializer.Write(m_CurrentSaveData, OutputOptions.None, Serializer.Format.JSON);
            Debug.LogFormat("[DataService] Current Profile: {0}", json);

            #endif // UNITY_EDITOR
        }

        #if UNITY_EDITOR

        private void BookmarkSaveData()
        {
            SyncProfile();

            Cursor.visible = true;
            
            string path = UnityEditor.EditorUtility.SaveFilePanelInProject("Save Bookmark", string.Empty, "json", "Choose a location to save your bookmark", "Assets/Resources/Bookmarks/");
            if (!string.IsNullOrEmpty(path))
            {
                Serializer.WriteFile(m_CurrentSaveData, path, OutputOptions.PrettyPrint, Serializer.Format.JSON);
                Debug.LogFormat("[DataService] Saved bookmark at {0}", path);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
                RegenerateBookmarks(m_BookmarksMenu);
            }

            Cursor.visible = false;
        }

        #endif // UNITY_EDITOR

        #region IDebuggable

        IEnumerable<DMInfo> IDebuggable.ConstructDebugMenus()
        {
            // jobs menu

            DMInfo jobsMenu = DebugService.NewDebugMenu("Jobs");

            DMInfo startJobMenu = DebugService.NewDebugMenu("Start Job");
            foreach(var job in Services.Assets.Jobs.Objects)
                RegisterJobStart(startJobMenu, job.Id());

            jobsMenu.AddSubmenu(startJobMenu);
            jobsMenu.AddDivider();
            jobsMenu.AddButton("Complete Current Job", () => Services.Data.Profile.Jobs.MarkComplete(Services.Data.CurrentJob()), () => !Services.Data.CurrentJobId().IsEmpty);
            jobsMenu.AddButton("Clear All Job Progress", () => Services.Data.Profile.Jobs.ClearAll());

            yield return jobsMenu;

            // bestiary menu

            DMInfo bestiaryMenu = DebugService.NewDebugMenu("Bestiary");

            DMInfo critterMenu = DebugService.NewDebugMenu("Critters");
            foreach(var critter in Services.Assets.Bestiary.AllEntriesForCategory(BestiaryDescCategory.Critter))
                RegisterEntityToggle(critterMenu, critter.Id());

            DMInfo envMenu = DebugService.NewDebugMenu("Environments");
            foreach(var env in Services.Assets.Bestiary.AllEntriesForCategory(BestiaryDescCategory.Environment))
                RegisterEntityToggle(envMenu, env.Id());

            DMInfo factMenu = DebugService.NewDebugMenu("Facts");
            Dictionary<StringHash32, DMInfo> factSubmenus = new Dictionary<StringHash32, DMInfo>();
            foreach(var fact in Services.Assets.Bestiary.AllFacts())
            {
                if (Services.Assets.Bestiary.IsAutoFact(fact.Id()))
                    continue;

                DMInfo submenu;
                StringHash32 submenuKey = fact.Parent().Id();
                if (!factSubmenus.TryGetValue(submenuKey, out submenu))
                {
                    submenu = DebugService.NewDebugMenu(submenuKey.ToDebugString());
                    factSubmenus.Add(submenuKey, submenu);
                    factMenu.AddSubmenu(submenu);
                }

                RegisterFactToggle(submenu, fact.Id());
            }

            bestiaryMenu.AddSubmenu(critterMenu);
            bestiaryMenu.AddSubmenu(envMenu);
            bestiaryMenu.AddSubmenu(factMenu);

            bestiaryMenu.AddDivider();

            bestiaryMenu.AddButton("Unlock All Entries", () => UnlockAllBestiaryEntries(false));
            bestiaryMenu.AddButton("Unlock All Facts", () => UnlockAllBestiaryEntries(true));
            bestiaryMenu.AddButton("Clear Bestiary", () => ClearBestiary());

            yield return bestiaryMenu;

            // map menu

            DMInfo mapMenu = DebugService.NewDebugMenu("World Map");

            foreach(var map in Services.Assets.Map.Stations())
            {
                RegisterStationToggle(mapMenu, map.Id());
            }

            mapMenu.AddDivider();

            mapMenu.AddButton("Unlock All Stations", () => UnlockAllStations());

            yield return mapMenu;

            // save data menu

            DMInfo saveMenu = DebugService.NewDebugMenu("Player Profile");

            DMInfo bookmarkMenu = DebugService.NewDebugMenu("Bookmarks");
            #if UNITY_EDITOR
            m_BookmarksMenu = bookmarkMenu;
            #endif // UNITY_EDITOR

            RegenerateBookmarks(bookmarkMenu);
            saveMenu.AddSubmenu(bookmarkMenu);
            saveMenu.AddDivider();

            saveMenu.AddButton("Save", () => SaveProfile("DEBUG"), IsProfileLoaded);
            saveMenu.AddButton("Save (Debug)", () => DebugSaveData(), IsProfileLoaded);
            #if UNITY_EDITOR
            saveMenu.AddButton("Save as Bookmark", () => BookmarkSaveData(), IsProfileLoaded);
            #else 
            saveMenu.AddButton("Save as Bookmark", null, () => false);
            #endif // UNITY_EDITOR
            saveMenu.AddToggle("Autosave Enabled", AutosaveEnabled, SetAutosaveEnabled);

            saveMenu.AddDivider();

            #if DEVELOPMENT
            saveMenu.AddButton("Reload Save", () => ForceReloadSave(), IsProfileLoaded);
            saveMenu.AddButton("Restart from Beginning", () => ForceRestart());
            #endif // DEVELOPMENT

            saveMenu.AddDivider();

            saveMenu.AddButton("Clear Local Saves", () => ClearLocalSaves());

            yield return saveMenu;
        }

        static private void RegenerateBookmarks(DMInfo inMenu)
        {
            var allBookmarks = Resources.LoadAll<TextAsset>("Bookmarks");

            inMenu.Clear();

            if (allBookmarks.Length == 0)
            {
                inMenu.AddText("No bookmarks :(", () => string.Empty);
            }
            else if (allBookmarks.Length <= 10)
            {
                foreach(var bookmark in allBookmarks)
                {
                    RegisterBookmark(inMenu, bookmark);
                }
            }
            else
            {
                int pageNumber = 0;
                int bookmarkCounter = 0;
                DMInfo page = new DMInfo("Page 1", 10);
                inMenu.AddSubmenu(page);
                foreach(var bookmark in allBookmarks)
                {
                    if (bookmarkCounter >= 10)
                    {
                        pageNumber++;
                        page = new DMInfo("Page " + pageNumber.ToString(), 10);
                        inMenu.AddSubmenu(page);
                    }

                    RegisterBookmark(page, bookmark);

                    bookmarkCounter++;
                }
            }
        }

        static private void RegisterBookmark(DMInfo inMenu, TextAsset inAsset)
        {
            #if DEVELOPMENT
            string name = inAsset.name;
            inMenu.AddButton(name, () => Services.Data.LoadBookmark(name), () => Services.Data.m_SaveResult == null);
            #endif // DEVELOPMENT
            
            Resources.UnloadAsset(inAsset);
        }

        static private void RegisterJobStart(DMInfo inMenu, StringHash32 inJobId)
        {
            inMenu.AddButton(inJobId.ToDebugString(), () => 
            {
                Services.Data.Profile.Jobs.ForgetJob(inJobId);
                Services.Data.Profile.Jobs.SetCurrentJob(inJobId); 
            }, () => Services.Data.CurrentJobId() != inJobId);
        }

        static private void RegisterEntityToggle(DMInfo inMenu, StringHash32 inEntityId)
        {
            inMenu.AddToggle(inEntityId.ToDebugString(),
                () => { return Services.Data.Profile.Bestiary.HasEntity(inEntityId); },
                (b) =>
                {
                    if (b)
                        Services.Data.Profile.Bestiary.RegisterEntity(inEntityId);
                    else
                        Services.Data.Profile.Bestiary.DeregisterEntity(inEntityId);
                }
            );
        }

        static private void RegisterFactToggle(DMInfo inMenu, StringHash32 inFactId)
        {
            inMenu.AddToggle(inFactId.ToDebugString(),
                () => { return Services.Data.Profile.Bestiary.HasFact(inFactId); },
                (b) =>
                {
                    if (b)
                        Services.Data.Profile.Bestiary.RegisterFact(inFactId, true);
                    else
                        Services.Data.Profile.Bestiary.DeregisterFact(inFactId);
                }
            );
        }


        static private void UnlockAllBestiaryEntries(bool inbIncludeFacts)
        {
            foreach(var entry in Services.Assets.Bestiary.Objects)
            {
                Services.Data.Profile.Bestiary.RegisterEntity(entry.Id());
                if (inbIncludeFacts)
                {
                    foreach(var fact in entry.Facts)
                        Services.Data.Profile.Bestiary.RegisterFact(fact.Id());
                }
            }
        }

        static private void ClearBestiary()
        {
            foreach(var entry in Services.Assets.Bestiary.Objects)
            {
                Services.Data.Profile.Bestiary.DeregisterEntity(entry.Id());
                foreach(var fact in entry.Facts)
                    Services.Data.Profile.Bestiary.DeregisterFact(fact.Id());
            }
        }

        static private void RegisterStationToggle(DMInfo inMenu, StringHash32 inStationId)
        {
            inMenu.AddToggle(inStationId.ToDebugString(),
                () => { return Services.Data.Profile.Map.IsStationUnlocked(inStationId); },
                (b) =>
                {
                    if (b)
                        Services.Data.Profile.Map.UnlockStation(inStationId);
                    else
                        Services.Data.Profile.Map.LockStation(inStationId);
                }
            );
        }

        static private void UnlockAllStations()
        {
            foreach(var map in Services.Assets.Map.Stations())
            {
                Services.Data.Profile.Map.UnlockStation(map.Id());
            }
        }

        static private void ClearLocalSaves()
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Log.Warn("[DataService] All local save data has been cleared");
        }

        #endregion // IDebuggable
    }
}