using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEditor.SceneManagement;
using System;

public class QuickSceneSwitcher : EditorWindow
{
    [MenuItem("Window/Utilities/Quick Scene Switcher")]
    public static void ShowWindow()
    {
        QuickSceneSwitcher window = GetWindow<QuickSceneSwitcher>("Quick scene switcher");
        
        window.Init();
    }
    
    [Serializable]
    private struct AssetNamePath
    {
        public string name;
        public string path;
        public bool enabled;
    }

    [SerializeField]
    private List<AssetNamePath> _allScenes;
    [SerializeField]
    private List<AssetNamePath> _buildScenes;
    [SerializeField]
    private string _firstScene;
    [SerializeField]
    private string _lastOpenScene;


    private void Init() {
        // Debug.Log("Init");
        _allScenes = FindAssetsByType<SceneAsset>("Scene");
        
        EditorBuildSettingsScene[] buildSettingsScenes = EditorBuildSettings.scenes;

        _buildScenes = new List<AssetNamePath>();
        _firstScene = string.Empty;

        for (int i = 0; i < buildSettingsScenes.Length; i++)
        {
            EditorBuildSettingsScene s = buildSettingsScenes[i];

            _buildScenes.Add(new AssetNamePath { name = nameFromPath(s.path), path = s.path, enabled=s.enabled });

            if (s.enabled && string.IsNullOrEmpty(_firstScene)) _firstScene = s.path;
               
        }
       
    }

    Vector2 _scrollPos;
    bool _buildOnly = true;

    void OnGUI()
    {
        _scrollPos = GUILayout.BeginScrollView(_scrollPos);

        if (_allScenes != null && _buildScenes != null) drawGUI();

        if (GUILayout.Button("REFRESH"))
        {
            Init();
        }

        GUILayout.EndScrollView();
    }

    private void drawGUI()
    {

        if (!EditorApplication.isPlaying)
        {
           
            if (GUILayout.Button("▶ PLAY FIRST ACTIVE SCENE"))
            {
                _lastOpenScene = EditorSceneManager.GetActiveScene().path;
                Debug.Log(_lastOpenScene);
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<SceneAsset>(_firstScene));
                EditorApplication.isPlaying = true;
            }
        }
        else
        {
            if (GUILayout.Button("■ RETURN TO SCENE"))
            {
                EditorApplication.isPlaying = false;
                Debug.Log(_lastOpenScene);
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<SceneAsset>(_lastOpenScene));
            }
        }

        GUILayout.Space(10);
        _buildOnly = GUILayout.Toggle(_buildOnly, "ONLY SHOW SCENES IN BUILD");
        
        GUILayout.Space(10);     
        List<AssetNamePath> assetList = _buildScenes;

        bool showEnabledState = true;
        
        if (!_buildOnly)
        {
            assetList = _allScenes;
            showEnabledState = false;
        }

        for (int i = 0; i < assetList.Count; i++)
        {
            AssetNamePath scene = assetList[i];

            string label = scene.name;
            
            if (showEnabledState)
            {

                label = (scene.enabled ? "[✓] " : "[✗] ") + label;
                if (scene.path == _firstScene)
                    label += " (FIRST)";
                
            
            }
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Open"))
            {
                AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath(scene.path, typeof(SceneAsset)) as SceneAsset);
            }
            if (GUILayout.Button("Ping"))
            {
                EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(scene.path, typeof(SceneAsset)) as SceneAsset);
            }
            if (GUILayout.Button("Select"))
            {
                SceneAsset sa = AssetDatabase.LoadAssetAtPath(scene.path, typeof(SceneAsset)) as SceneAsset;
                Selection.activeObject = sa;
                // EditorGUIUtility.PingObject(sa);
            }
                     
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }

    }

    private static List<AssetNamePath> FindAssetsByType<T>(string typeName) where T : UnityEngine.Object
    {
        List<AssetNamePath> assets = new List<AssetNamePath>();
        string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeName));
        for (int i = 0; i < guids.Length; i++)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            
            if (asset != null)
            {
                assets.Add(new AssetNamePath { name=asset.name, path=assetPath });
            }
        }
        return assets;
    }

    private static string nameFromPath(string path)
    {
        int slash = path.LastIndexOf('/');
        string name = path.Substring(slash + 1);
        int dot = name.LastIndexOf('.');
        return name.Substring(0, dot);
    }
}
