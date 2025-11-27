#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DGP.EntryPoints.Editor
{
    [InitializeOnLoad]
    public static class PlayModeOptions
    {
        private static GUIStyle popupStyle;
        private static List<string> cachedEntryPointPaths;
        private static bool needsRefresh = true;
        
        private const string ConfigPrefsKey = "EntryPoint_ActiveConfig";
        
        private static string SelectedConfigPath
        {
            get => EditorPrefs.GetString(ConfigPrefsKey, string.Empty);
            set => EditorPrefs.SetString(ConfigPrefsKey, value);
        }

        static PlayModeOptions()
        {
            EditorApplication.update += DelayedInitialize;
        }

        private static void DelayedInitialize()
        {
            EditorApplication.update -= DelayedInitialize;
            
            if (!string.IsNullOrEmpty(SelectedConfigPath)) {
                var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(SelectedConfigPath) as IEntryPoint;
                config?.OnEntryPointSelected();
                EntryPoints.ActiveEntryPoint = config;
            }

            var toolbarType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");
            var toolbarFieldInfo = toolbarType.GetField("get", BindingFlags.Public | BindingFlags.Static);
            var toolbarObject = toolbarFieldInfo?.GetValue(null);

            if (toolbarObject != null) {
                var root = toolbarType.GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
                var rawRoot = root?.GetValue(toolbarObject);
                var mRoot = rawRoot as VisualElement;
                
                RegisterCallback("ToolbarZoneLeftAlign", OnToolbarGUI);

                void RegisterCallback(string rootName, Action cb)
                {
                    var toolbarZone = mRoot?.Q(rootName);
                    
                    if (toolbarZone != null) {
                        var parent = new VisualElement() {
                            style =
                            {
                                flexGrow = 1,
                                flexDirection = FlexDirection.Row,
                            }
                        };
                        var container = new IMGUIContainer();
                        container.style.flexGrow = 1;
                        container.onGUIHandler += () => cb?.Invoke();
                        parent.Add(container);
                        toolbarZone.Add(parent);
                    }
                }
            }
        }

        private static void InitializeStyle()
        {
            popupStyle = new GUIStyle(EditorStyles.popup) {
                fixedHeight = 21,
                fontSize = 12
            };
        }

        private static void OnToolbarGUI()
        {
            if (popupStyle == null)
                InitializeStyle();

            var entryPoints = GetOrderedEntryPoints();
            
            GUILayout.FlexibleSpace();
            
            // Always add [Current Scene] as first option
            var displayNames = new List<string> { "[Current Scene]" };
            displayNames.AddRange(entryPoints.Select(path => 
                ObjectNames.NicifyVariableName(System.IO.Path.GetFileNameWithoutExtension(path))));

            var selectedIndex = string.IsNullOrEmpty(SelectedConfigPath)
                ? 0  // Default to [Current Scene]
                : entryPoints.FindIndex(p => p == SelectedConfigPath) + 1;  // +1 because of [Current Scene]

            var newIndex = EditorGUILayout.Popup(selectedIndex, displayNames.ToArray(), popupStyle, GUILayout.Width(150));

            if (newIndex != selectedIndex) {
                if (newIndex == 0) {
                    // [Current Scene] selected - clear startup scene
                    SelectedConfigPath = string.Empty;
                    EntryPoints.ActiveEntryPoint = null;
                    UnityEditor.SceneManagement.EditorSceneManager.playModeStartScene = null;
                } else {
                    // Entry point selected
                    SelectedConfigPath = entryPoints[newIndex - 1];  // -1 because of [Current Scene]
                    var config = AssetDatabase.LoadAssetAtPath<ScriptableObject>(SelectedConfigPath) as IEntryPoint;
                    config?.OnEntryPointSelected();
                    EntryPoints.ActiveEntryPoint = config;
                }
            }
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
    }

    public class EntryPointAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var allChangedAssets = importedAssets.Concat(movedAssets).Concat(movedFromAssetPaths).Concat(deletedAssets);

            foreach (var assetPath in allChangedAssets) {
                if (!assetPath.EndsWith(".asset")) 
                    continue;
                
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
                if (asset is IEntryPoint || deletedAssets.Contains(assetPath)) {
                    typeof(PlayModeOptions)
                        .GetField("needsRefresh", BindingFlags.Static | BindingFlags.NonPublic)
                        ?.SetValue(null, true);
                    return;
                }
            }
        }
    }
}
#endif