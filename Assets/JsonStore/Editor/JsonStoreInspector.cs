using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

[CustomEditor(typeof(JsonStoreAsset))]
public class JsonStoreInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var asset = target as JsonStoreAsset;
        if (asset.list == null)
            return;

        foreach (var i in asset.list)
        {
            if (GUILayout.Button(i.name))
            {
                JsonStoreWindow.target = i;
                JsonStoreWindow.list = asset.list;
                EditorWindow.GetWindow<JsonStoreWindow>("Json Store");
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