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
    private const int ListWidth = 150;

    private TextAsset _asset;
    private JsonDictonary _data;

    private string _selectedKey = "";
    private string _newKey = "";
    private List<string> _multiSelectedKey = new List<string>();

    private int _createCount = 1;

    private Vector2 _listScrollPos = Vector2.zero;
    private Vector2 _itemScrollPos = Vector2.zero;

    private KeyCode _pressedKeyCode = KeyCode.None;

    private bool _isDuplicated = false;

    private string _searchStr = "";

    void OnGUI()
    {
        var e = Event.current;
        switch (e.type)
        {
            case EventType.keyDown:
                _pressedKeyCode = e.keyCode;
                SelectUpDown(e);
                break;
            case EventType.keyUp:
                _pressedKeyCode = KeyCode.None;
                break;
        }

        OnToolbarGUI();

        GUILayout.BeginHorizontal();

        OnListGUI();
        OnEditorGUI();

        GUILayout.EndHorizontal();
    }

    public void OnInspectorUpdate()
    {
        Repaint();
    }
    
    public void Load(TextAsset asset)
    {
        _asset = asset;
        _data = JsonConverter.ToJsonObject(asset.text).AsDictonary;
    }

    public void Create()
    {
        GUI.FocusControl("");

        string key = (_data.Count).ToString();
        while (_data.Contains(key))
            key = (int.Parse(key) + 1).ToString();

        _data.Add(key, JsonToJsonObject.Convert("{}"));
        _selectedKey = _newKey = key;
    }

    public void Copy()
    {
        GUI.FocusControl("");

        string key = (_data.Count).ToString();
        if (_data.Contains(key))
            key = (_data.Count + 1).ToString();
        if (_data.Contains(key))
        {
            key = _selectedKey;
            while (_data.Contains(key))
                key = string.Format("{0} copy", key);
        }

        var copyStr = _data[_selectedKey].AsDictonary.ToString();
        var copy = SolJSON.Convert.Helper.JsonToJsonObject.Convert(copyStr);
        _data.Add(key, copy);
        _selectedKey = _newKey = key;
    }

    public void Delete()
    {
        GUI.FocusControl("");

        if (!_data.Contains(_selectedKey))
            return;

        if (!EditorUtility.DisplayDialog("Delete", "?", "Delete", "Cancel"))
            return;

        _data.Remove(_selectedKey);
        foreach (var key in _multiSelectedKey)
        {
            _data.Remove(key);
        }
        _selectedKey = "";
        _newKey = "";
    }
    
    public void Commit()
    {
        GUI.FocusControl("");
        
        string assetpath = AssetDatabase.GetAssetPath(_asset);
        string jsonstr = _data.ToString(4);
        StreamWriter file = new StreamWriter(assetpath);
        file.Write(jsonstr);
        file.Close();
        EditorUtility.DisplayDialog("Saved", "OK", "OK");
        AssetDatabase.Refresh();
    }

    public void SelectUpDown(Event e)
    {
        if (e.keyCode != KeyCode.UpArrow
            && e.keyCode != KeyCode.DownArrow)
            return;

        _multiSelectedKey.Clear();

        GUI.FocusControl("");
        if (e.keyCode == KeyCode.UpArrow)
        {
            string prevKey = _selectedKey;
            foreach (var pair in _data)
            {
                if (pair.Key == _selectedKey)
                {
                    _selectedKey = _newKey = prevKey;
                    break;
                }
                prevKey = pair.Key;
            }
        }
        else if (e.keyCode == KeyCode.DownArrow)
        {
            bool find = false;
            foreach (var pair in _data)
            {
                if (find)
                {
                    _selectedKey = _newKey = pair.Key;
                    break;
                }
                find = pair.Key == _selectedKey;
            }
        }
    }

    public List<string> GetJsonDictKeys()
    {
        var keys = new List<string>();
        foreach (var pair in _data)
        {
            keys.Add(pair.Key);
        }
        return keys;
    }

    public void MoveSelectedKey(int value)
    {
        var keys = GetJsonDictKeys();
        var index = keys.IndexOf(_selectedKey);
        var multiSelectDown = false;
        foreach (var key in _multiSelectedKey)
        {
            multiSelectDown = index < keys.IndexOf(key);
            break;
        }

        if (multiSelectDown && value > 0)
            index += _multiSelectedKey.Count + value;
        if (multiSelectDown && value < 0)
            index += value;
        if (!multiSelectDown && value > 0)
            index += value;
        if (!multiSelectDown && value < 0)
        {
            index -= _multiSelectedKey.Count;
            index += value;
        }

        if (index < 0 || index >= keys.Count)
            return;

        var removeKey = keys[index];
        keys.RemoveAt(index);

        index = keys.IndexOf(_selectedKey);
        if (multiSelectDown && value < 0)
            index += _multiSelectedKey.Count - value;
        if (!multiSelectDown && value > 0)
            index -= _multiSelectedKey.Count;
        if (!multiSelectDown && value < 0)
            index -= value;

        index = Math.Max(index, 0);
        index = Math.Min(index, keys.Count);
        
        keys.Insert(index, removeKey);
        
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            newDict.Add(key, _data[key]);
        }
        _data = newDict;
    }
    
    public void MoveToTop()
    {
        var keys = GetJsonDictKeys();
        keys.Remove(_selectedKey);
        keys.Insert(0, _selectedKey);
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            newDict.Add(key, _data[key]);
        }
        _data = newDict;
    }
    
    public void MoveToBottom()
    {
        var keys = GetJsonDictKeys();
        keys.Remove(_selectedKey);
        keys.Add(_selectedKey);
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            newDict.Add(key, _data[key]);
        }
        _data = newDict;
    }

    public void Up()
    {
        MoveSelectedKey(-1);
    }

    public void Down()
    {
        MoveSelectedKey(1);
    }

    private void OnToolbarGUI()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));

        GUILayout.Label("Count:");
        _createCount = EditorGUILayout.IntField(_createCount, EditorStyles.toolbarTextField, GUILayout.Width(26));
        if (GUILayout.Button("Create", EditorStyles.toolbarButton))
        {
            for (int i = 0; i < _createCount; ++i)
                Create();
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Open Data Fill Helper", EditorStyles.toolbarButton))
        {
            DataFillHelper win = EditorWindow.GetWindow<DataFillHelper>(_asset.name);
            win.SetData(_asset, _data, _selectedKey);
        }

        GUILayout.FlexibleSpace();

        GUILayout.Label("Search");
        _searchStr = GUILayout.TextField(_searchStr, GUILayout.Width(100));
        if (GUILayout.Button("x", EditorStyles.toolbarButton))
        {
            _searchStr = "";
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Commit", EditorStyles.toolbarButton))
            Commit();

        GUILayout.EndHorizontal();
    }

    private void OnListGUI()
    {
        if (_asset == null
            || _data == null)
        {
            Close();
            return;
        }

        GUILayout.BeginVertical(GUILayout.Width(ListWidth));

        _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos, GUILayout.Width(ListWidth), GUILayout.Height(position.height - 20));

        var list = new List<string>();
        if (_searchStr != "")
        {
            foreach (var pair in _data)
            {
                foreach (var p in pair.Value.AsDictonary)
                {
                    if (p.Value.Type == JsonObject.TYPE.STRING
                             && p.Value.AsString.Value.IndexOf(_searchStr) >= 0)
                    {
                        list.Add(pair.Key);
                        break;
                    }
                }
            }
        }
        else
        {
            foreach (var pair in _data) {
                list.Add(pair.Key);
            }
        }

        foreach (var key in list)
        {
            if (key == _selectedKey)
                GUI.contentColor = Color.green;

            if (_multiSelectedKey.Contains(key))
                GUI.contentColor = Color.yellow;

            if (GUILayout.Button(key))
            {
                GUI.FocusControl("");
                _multiSelectedKey.Clear();

                if (_pressedKeyCode == KeyCode.LeftControl)
                {
                    var keys = GetJsonDictKeys();
                    var begin = keys.IndexOf(_selectedKey);
                    var end = keys.IndexOf(key);
                    if (begin < end)
                    {
                        begin += 1;
                        end += 1;
                        _multiSelectedKey.AddRange(keys.GetRange(begin, end - begin));
                    }
                    else if (begin > end)
                    {
                        _multiSelectedKey.AddRange(keys.GetRange(end, begin - end));
                    }
                }
                else
                {
                    _selectedKey = key;
                    _newKey = key;
                }
            }
            GUI.contentColor = Color.white;
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void OnEditorGUI()
    {
        if (!_data.Contains(_selectedKey))
            return;

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy"))
            Copy();

        GUILayout.Space(10);

        GUILayout.Label("Move:");
        if (GUILayout.Button("Up"))
            Up();
        if (GUILayout.Button("Down"))
            Down();

        if (_multiSelectedKey.Count == 0)
        {
            if (GUILayout.Button("Top"))
                MoveToTop();
            if (GUILayout.Button("Bottom"))
                MoveToBottom();
        }

        GUILayout.FlexibleSpace();

        GUI.color = Color.red;
        if (GUILayout.Button("Delete"))
            Delete();
        GUI.color = Color.white;

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Key", GUILayout.Width(40));
        _newKey = GUILayout.TextField(_newKey, GUILayout.Width(100));
        if (_isDuplicated)
        {
            GUI.contentColor = Color.red;
            GUILayout.Label("Duplicated Key!");
            GUI.contentColor = Color.white;
        }
        _isDuplicated = false;
        if (_selectedKey != _newKey)
        {
            ChangeKey();
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            return;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        _itemScrollPos = EditorGUILayout.BeginScrollView(_itemScrollPos, GUILayout.Width (position.width - ListWidth), GUILayout.Height(position.height - 70));

        if (!_data.Contains(_selectedKey))
            return;

        var orig = _data[_selectedKey].AsDictonary;
        var sobj = JsonStore.ToScriptableObject(_asset.name, orig);

        EditorGUI.BeginChangeCheck();

        var editor = Editor.CreateEditor(sobj);
        if (editor != null)
            editor.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            _data[_selectedKey] = JsonStore.ToJsonObject(sobj);

            var dict = JsonStore.ToJsonObject(sobj).AsDictonary;
            foreach (var pair in orig)
            {
                if (dict[pair.Key].ToString() != pair.Value.ToString())
                {
                    foreach (var key in _multiSelectedKey)
                    {
                        _data[key].AsDictonary[pair.Key] = dict[pair.Key];
                    }
                }
            }
        }

        EditorGUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    private void ChangeKey()
    {
        if (_data.Contains(_newKey))
        {
            _isDuplicated = true;
            return;
        }
        var keys = GetJsonDictKeys();
        var index = keys.IndexOf(_selectedKey);
        keys.Remove(_selectedKey);
        keys.Insert(index, _newKey);
        var newDict = new JsonDictonary();
        foreach (var key in keys)
        {
            if (_newKey == key)
                newDict.Add(key, _data[_selectedKey]);
            else
                newDict.Add(key, _data[key]);
        }
        _data = newDict;
        _selectedKey = _newKey;
    }
}
