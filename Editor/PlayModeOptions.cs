#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace DGP.EntryPoints.Editor
{
    public class PlayModeOptions
    {
        private const string ElementPath = "EntryPoint/Selector";
        private const string ConfigPrefsKey = "EntryPoint_ActiveConfig";
        
        private static List<string> cachedEntryPointPaths;
        private static bool needsRefresh = true;
        
        private static string SelectedConfigPath
        {
            get => EditorPrefs.GetString(ConfigPrefsKey, string.Empty);
            set => EditorPrefs.SetString(ConfigPrefsKey, value);
        }

        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            if (!string.IsNullOrEmpty(SelectedConfigPath)) {
                var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(SelectedConfigPath) as IEntryPoint;
                config?.OnEntryPointSelected();
                EntryPoints.ActiveEntryPoint = config;
            }
        }

        [MainToolbarElement(ElementPath, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static MainToolbarElement CreateEntryPointSelector()
        {
            var currentName = GetCurrentEntryPointName();
            var icon = EditorGUIUtility.IconContent("d_SceneAsset Icon").image as Texture2D;
            var content = new MainToolbarContent(currentName, icon, "Select Entry Point");
            
            return new MainToolbarDropdown(content, ShowDropdown);
        }

        private static void ShowDropdown(Rect dropdownRect)
        {
            var menu = new GenericMenu();
            var entryPoints = GetOrderedEntryPoints();
            
            // Add [Current Scene] option
            menu.AddItem(
                new GUIContent("[Current Scene]"),
                string.IsNullOrEmpty(SelectedConfigPath),
                () => SelectEntryPoint(null)
            );
            
            menu.AddSeparator("");
            
            // Add all entry points
            foreach (var path in entryPoints) {
                var displayName = ObjectNames.NicifyVariableName(System.IO.Path.GetFileNameWithoutExtension(path));
                menu.AddItem(
                    new GUIContent(displayName),
                    SelectedConfigPath == path,
                    () => SelectEntryPoint(path)
                );
            }
            
            menu.DropDown(dropdownRect);
        }

        private static void SelectEntryPoint(string path)
        {
            if (string.IsNullOrEmpty(path)) {
                // [Current Scene] selected - clear startup scene
                SelectedConfigPath = string.Empty;
                EntryPoints.ActiveEntryPoint = null;
                UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = null;
            } else {
                // Entry point selected
                SelectedConfigPath = path;
                var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) as IEntryPoint;
                config?.OnEntryPointSelected();
                EntryPoints.ActiveEntryPoint = config;
            }
            
            // Refresh the toolbar element to update the displayed text
            MainToolbar.Refresh(ElementPath);
        }

        private static string GetCurrentEntryPointName()
        {
            if (string.IsNullOrEmpty(SelectedConfigPath))
                return "[Current Scene]";
            
            return ObjectNames.NicifyVariableName(
                System.IO.Path.GetFileNameWithoutExtension(SelectedConfigPath)
            );
        }

        private static List<string> GetOrderedEntryPoints()
        {
            if (!needsRefresh && cachedEntryPointPaths != null)
                return cachedEntryPointPaths;

            RefreshEntryPointsCache();
            needsRefresh = false;

            return cachedEntryPointPaths;
        }

        private static void RefreshEntryPointsCache()
        {
            cachedEntryPointPaths = AssetDatabase.FindAssets("t:ScriptableObject")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path =>
                {
                    var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
                    return asset is IEntryPoint;
                })
                .OrderBy(path => ObjectNames.NicifyVariableName(System.IO.Path.GetFileNameWithoutExtension(path)))
                .ToList();
        }
        
        public static void MarkForRefresh()
        {
            needsRefresh = true;
        }
    }

    public class EntryPointAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets, 
            string[] deletedAssets, 
            string[] movedAssets, 
            string[] movedFromAssetPaths)
        {
            var allChangedAssets = importedAssets
                .Concat(movedAssets)
                .Concat(movedFromAssetPaths)
                .Concat(deletedAssets);

            foreach (var assetPath in allChangedAssets) {
                if (!assetPath.EndsWith(".asset")) 
                    continue;
                
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (asset is IEntryPoint || deletedAssets.Contains(assetPath)) {
                    PlayModeOptions.MarkForRefresh();
                    return;
                }
            }
        }
    }
}
#endif