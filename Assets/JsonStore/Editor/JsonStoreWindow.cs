using UnityEngine;
using UnityEditor;
using SolJSON.Convert;
using SolJSON.Convert.Helper;
using SolJSON.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

public class JsonStoreWindow : EditorWindow
{
    private TextAsset asset;
    private JsonDictonary jsonDict;
    private string selectedKey = "";
    private string newKey = "";
    private int createCount = 1;
    private Vector2 listScrollPos = Vector2.zero;
    private Vector2 itemScrollPos = Vector2.zero;
    private List<string> multiSelectedKey = new List<string>();
    private KeyCode pressedKeyCode;
    private bool isDuplicated = false;
    private string searchStr = "";
    
    public void SetAsset(TextAsset asset)
    {
        this.asset = asset;
        this.jsonDict = JsonConverter.ToJsonObject(asset.text).AsDictonary;
    }

    void OnGUI()
    {
        var e = Event.current;
        switch (e.type)
        {
            case EventType.keyDown:
                pressedKeyCode = e.keyCode;
                SelectUpDown(e);
                break;
            case EventType.keyUp:
                pressedKeyCode = KeyCode.None;
                break;
        }

        DrawToolbar();
        GUILayout.BeginHorizontal();
        DrawDataList();
        DrawDataItem();
        GUILayout.EndHorizontal();
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }

    public void Create()
    {
        GUI.FocusControl("");

        string key = (jsonDict.Count).ToString();
        while (jsonDict.Contains(key))
            key = (int.Parse(key) + 1).ToString();

        jsonDict.Add(key, JsonToJsonObject.Convert("{}"));
        selectedKey = newKey = key;
    }
    
    public void Commit()
    {
        GUI.FocusControl("");

        string assetpath = AssetDatabase.GetAssetPath(asset);
        string jsonstr = jsonDict.ToString(4);
        StreamWriter file = new StreamWriter(assetpath);
        file.Write(jsonstr);
        file.Close();
        EditorUtility.DisplayDialog("Saved", "OK", "OK");
        AssetDatabase.Refresh();
    }

    public void Copy()
    {
        GUI.FocusControl("");

        string key = (jsonDict.Count).ToString();
        if (jsonDict.Contains(key))
            key = (jsonDict.Count + 1).ToString();
        if (jsonDict.Contains(key))
        {
            key = selectedKey;
            while (jsonDict.Contains(key))
                key = string.Format("{0} copy", key);
        }

        var copyStr = jsonDict[selectedKey].AsDictonary.ToString();
        var copy = SolJSON.Convert.Helper.JsonToJsonObject.Convert(copyStr);
        jsonDict.Add(key, copy);
        selectedKey = newKey = key;
    }

    public void Delete()
    {
        GUI.FocusControl("");

        if (!jsonDict.Contains(selectedKey))
            return;

        if (!EditorUtility.DisplayDialog("Delete", "?", "Delete", "Cancel"))
            return;

        jsonDict.Remove(selectedKey);
        foreach (var key in multiSelectedKey)
        {
            jsonDict.Remove(key);
        }
        selectedKey = "";
        newKey = "";
    }

    public void SelectUpDown(Event e)
    {
        if (e.keyCode != KeyCode.UpArrow
            && e.keyCode != KeyCode.DownArrow)
            return;

        multiSelectedKey.Clear();   


        GUI.FocusControl("");
        if (e.keyCode == KeyCode.UpArrow)
        {
            string prevKey = selectedKey;
            foreach (var pair in jsonDict)
            {
                if (pair.Key == selectedKey)
                {
                    selectedKey = newKey = prevKey;
                    break;
                }
                prevKey = pair.Key;
            }
        }
        else if (e.keyCode == KeyCode.DownArrow)
        {
            bool find = false;
            foreach (var pair in jsonDict)
            {
                if (find)
                {
                    selectedKey = newKey = pair.Key;
                    break;
                }
                find = pair.Key == selectedKey;
            }
        }
    }

    public List<string> GetJsonDictKeys()
    {
        var keys = new List<string>();
        foreach (var pair in jsonDict)
        {
            keys.Add(pair.Key);
        }
        return keys;
    }

    public void MoveSelectedKey(int value)
    {
        var keys = GetJsonDictKeys();
        var index = keys.IndexOf(selectedKey);
        var multiSelectDown = false;
        foreach (var key in multiSelectedKey)
        {
            multiSelectDown = index < keys.IndexOf(key);
            break;
        }

        if (multiSelectDown && value > 0)
            index += multiSelectedKey.Count + value;
        if (multiSelectDown && value < 0)
            index += value;
        if (!multiSelectDown && value > 0)
            index += value;
        if (!multiSelectDown && value < 0)
        {
            index -= multiSelectedKey.Count;
            index += value;
        }

        if (index < 0 || index >= keys.Count)
            return;

        var removeKey = keys[index];
        keys.RemoveAt(index);

        index = keys.IndexOf(selectedKey);
        if (multiSelectDown && value < 0)
            index += multiSelectedKey.Count - value;
        if (!multiSelectDown && value > 0)
            index -= multiSelectedKey.Count;
        if (!multiSelectDown && value < 0)
            index -= value;

        index = Math.Max(index, 0);
        index = Math.Min(index, keys.Count);
        
        keys.Insert(index, removeKey);
        
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            newDict.Add(key, jsonDict[key]);
        }
        jsonDict = newDict;
    }
    
    public void MoveToTop()
    {
        var keys = GetJsonDictKeys();
        keys.Remove(selectedKey);
        keys.Insert(0, selectedKey);
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            newDict.Add(key, jsonDict[key]);
        }
        jsonDict = newDict;
    }
    
    public void MoveToBottom()
    {
        var keys = GetJsonDictKeys();
        keys.Remove(selectedKey);
        keys.Add(selectedKey);
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            newDict.Add(key, jsonDict[key]);
        }
        jsonDict = newDict;
    }

    public void Up()
    {
        MoveSelectedKey(-1);
    }

    public void Down()
    {
        MoveSelectedKey(1);
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

        if (GUILayout.Button("Create", EditorStyles.toolbarButton))
        {
            for (int i = 0; i < createCount; ++i)
                Create();
        }

        createCount = EditorGUILayout.IntField(createCount, GUILayout.Width(50));

        GUILayout.Space(20);

        GUILayout.Label("search string");
        searchStr = GUILayout.TextField(searchStr, GUILayout.Width(100));
        if (GUILayout.Button("x", EditorStyles.toolbarButton))
        {
            searchStr = "";
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Open Data Fill Helper", EditorStyles.toolbarButton))
        {
            DataFillHelper win = EditorWindow.GetWindow<DataFillHelper>(asset.name);
            win.SetData(asset, jsonDict, selectedKey);
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Commit", EditorStyles.toolbarButton))
            Commit();

        GUILayout.EndHorizontal();
    }

    private void DrawDataList()
    {
        if (asset == null
            || jsonDict == null)
        {
            Close();
            return;
        }

        GUILayout.BeginVertical(GUILayout.Width(120));

        listScrollPos = EditorGUILayout.BeginScrollView(listScrollPos, GUILayout.Width (120), GUILayout.Height(position.height - 20));

        var list = new List<string>();
        if (searchStr != "")
        {
            foreach (var pair in jsonDict)
            {
                foreach (var p in pair.Value.AsDictonary)
                {
                    if (p.Value.Type == JsonObject.TYPE.STRING
                             && p.Value.AsString.Value.IndexOf(searchStr) >= 0)
                    {
                        list.Add(pair.Key);
                        break;
                    }
                }
            }
        }
        else
        {
            foreach (var pair in jsonDict) {
                list.Add(pair.Key);
            }
        }

        foreach (var key in list)
        {
            if (key == selectedKey)
                GUI.contentColor = Color.green;

            if (multiSelectedKey.Contains(key))
                GUI.contentColor = Color.yellow;

            if (GUILayout.Button(key))
            {
                GUI.FocusControl("");
                multiSelectedKey.Clear();

                if (pressedKeyCode == KeyCode.LeftControl)
                {
                    var keys = GetJsonDictKeys();
                    var begin = keys.IndexOf(selectedKey);
                    var end = keys.IndexOf(key);
                    if (begin < end)
                    {
                        begin += 1;
                        end += 1;
                        multiSelectedKey.AddRange(keys.GetRange(begin, end - begin));
                    }
                    else if (begin > end)
                    {
                        multiSelectedKey.AddRange(keys.GetRange(end, begin - end));
                    }
                }
                else
                {
                    selectedKey = key;
                    newKey = key;
                }
            }
            GUI.contentColor = Color.white;
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void DrawDataItem()
    {
        if (!jsonDict.Contains(selectedKey))
            return;

        GUILayout.BeginVertical();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy"))
            Copy();

        GUILayout.Space(10);

        if (GUILayout.Button("Up"))
            Up();
        if (GUILayout.Button("Down"))
            Down();

        GUILayout.Space(10);

        if (multiSelectedKey.Count == 0)
        {
            if (GUILayout.Button("Top"))
                MoveToTop();
            if (GUILayout.Button("Bottom"))
                MoveToBottom();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Delete"))
            Delete();
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Key", GUILayout.Width(40));
        newKey = GUILayout.TextField(newKey, GUILayout.Width(100));
        if (isDuplicated)
        {
            GUI.contentColor = Color.red;
            GUILayout.Label("Duplicated Key!");
            GUI.contentColor = Color.white;
        }
        isDuplicated = false;
        if (selectedKey != newKey)
        {
            ChangeKey();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return;
        }
        GUILayout.EndHorizontal();

        itemScrollPos = EditorGUILayout.BeginScrollView(itemScrollPos, GUILayout.Width (position.width - 120), GUILayout.Height(position.height - 70));

        GUILayout.Space(10);
        
        if (!jsonDict.Contains(selectedKey))
            return;

        JsonDictonary j = jsonDict[selectedKey].AsDictonary;
        ScriptableObject o = JsonStore.ToScriptableObject(asset.name, j);

        EditorGUI.BeginChangeCheck();

        var editor = Editor.CreateEditor(o);
        if (editor != null)
            editor.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            jsonDict[selectedKey] = JsonStore.ToJsonObject(o);
            foreach (var key in multiSelectedKey)
            {
                jsonDict[key] = JsonStore.ToJsonObject(o);
            }
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void ChangeKey()
    {
        if (jsonDict.Contains(newKey))
        {
            isDuplicated = true;
            return;
        }
        var keys = GetJsonDictKeys();
        var index = keys.IndexOf(selectedKey);
        keys.Remove(selectedKey);
        keys.Insert(index, newKey);
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            if (newKey == key)
                newDict.Add(key, jsonDict[selectedKey]);
            else
                newDict.Add(key, jsonDict[key]);
        }
        jsonDict = newDict;
        selectedKey = newKey;
    }
}
