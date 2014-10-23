using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[CustomEditor(typeof(JsonStoreEditor))]
public class JsonStoreInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        JsonStoreEditor store = target as JsonStoreEditor;
        foreach (TextAsset asset in store.assets)
        {
            if (GUILayout.Button(asset.name))
            {
                JsonStoreWindow win = EditorWindow.GetWindow<JsonStoreWindow>(asset.name);
                win.title = asset.name;
                win.Load(asset);
            }
        }
    }

    [MenuItem("Assets/Create/JsonStore")]
    static void CreateJsonStore()
    {
        string path = AssetDatabase.GetAssetPath (Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        else if (Path.GetExtension(path) != "")
        {
            path = path.Replace(Path.GetFileName (AssetDatabase.GetAssetPath (Selection.activeObject)), "");
        }
        path = Path.Combine(path, "NewJsonStore.txt");
        StreamWriter file = File.CreateText(path);
        file.Write("{}");
        file.Close();
        AssetDatabase.Refresh();
    }
}