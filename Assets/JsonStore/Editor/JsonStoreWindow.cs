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
using System.Linq;

public class JsonStoreWindow : EditorWindow
{
    static public TextAsset target;
    static public TextAsset[] list;

    private readonly int ListWidth = 250;

    private JsonDictonary _data;
    
    private bool _useRefs = false;
    private Dictionary<string, List<string>> _refers;
    private Dictionary<string, List<string>> _refNames;

    private string _selectedKey = "";
    private string _newKey = "";
    private List<string> _multiSelectedKey = new List<string>();

    private int _createCount = 1;

    private Vector2 _listScrollPos = Vector2.zero;
    private Vector2 _itemScrollPos = Vector2.zero;

    private KeyCode _pressedKeyCode = KeyCode.None;

    private bool _isDuplicated = false;

    private string _searchStr = "";

    public void OnEnable()
    {
        if (target == null)
        {
            Close();
            return;
        }
        
        Load();
    }

    void OnGUI()
    {
        var e = Event.current;
        switch (e.type)
        {
            case EventType.keyDown:
                _pressedKeyCode = e.keyCode;
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
    
    public void Load()
    {
        _data = JsonConverter.ToJsonObject(target.text).AsDictonary;
        _refers = new Dictionary<string, List<string>>();
        _refNames = new Dictionary<string, List<string>>();
        foreach (var i in JsonStoreWindow.list)
        {
            _refers[i.name] = new List<string>();
            _refNames[i.name] = new List<string>();
            var data = JsonConverter.ToJsonObject(i.text).AsDictonary;
            foreach (var j in data)
            {
                _refers[i.name].Add(j.Key);

                if (j.Value.AsDictonary != null
                    && j.Value.AsDictonary.Contains("Name"))
                {
                    var name = j.Value.AsDictonary["Name"].AsString.Value;
                    _refNames[i.name].Add(string.Format("({0})", name));
                }
                else
                {
                    _refNames[i.name].Add("()");
                }
            }
        }
    }

    public void Create()
    {
        GUI.FocusControl("");

        string key = (_data.Count).ToString();
        while (_data.Contains(key))
            key = (int.Parse(key) + 1).ToString();

        _data.Add(key, JsonConverter.ToJsonObject("{}"));

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
        var copy = JsonConverter.ToJsonObject(copyStr);
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
        
        string assetpath = AssetDatabase.GetAssetPath(target);
        string jsonstr = _data.ToString(4).Replace("\r", "");
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

        if (GUILayout.Button("Data Helper", EditorStyles.toolbarButton))
        {
            var win = EditorWindow.GetWindow<DataFillHelper>(target.name);
            win.SetData(target, _data, _selectedKey);
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
        if (target == null
            || _data == null)
        {
            Close();
            return;
        }

        GUILayout.BeginVertical(GUILayout.Width(ListWidth));

        _listScrollPos = EditorGUILayout.BeginScrollView(_listScrollPos, GUILayout.Width(ListWidth), GUILayout.Height(position.height - 20));

        var nameList = new List<string>();
        var keyList = new List<string>();
        foreach (var pair in _data) {
            if (_searchStr != "")
            {
                var find = false;
                foreach (var p in pair.Value.AsDictonary)
                {
                    if (p.Value.Type == JsonObject.TYPE.STRING
                        && p.Value.AsString.Value.IndexOf(_searchStr) >= 0)
                        find = true;
                }
                if (!find)
                    continue;
            }
            
            if (pair.Value.AsDictonary != null && pair.Value.AsDictonary.Contains("Name"))
            {
                var namestr = pair.Value.AsDictonary["Name"].ToString();
                namestr = namestr.Replace("\"", "");
                namestr = namestr.Replace("\\r", "");
                nameList.Add(string.Format("{0} ({1})", pair.Key, namestr));
            }
            else
                nameList.Add(pair.Key);
            
            keyList.Add(pair.Key);
        }

        int idx = -1;
        foreach (var key in keyList)
        {
            idx++;
            if (key == _selectedKey)
                GUI.contentColor = Color.green;

            if (_multiSelectedKey.Contains(key))
                GUI.contentColor = Color.yellow;

            if (GUILayout.Button(nameList[idx]))
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

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();
        _createCount = EditorGUILayout.IntField(_createCount, GUILayout.Width(40));
        if (GUILayout.Button("+"))//string.Format("Create {0} Row", _createCount)))
        {
            for (int i = 0; i < _createCount; ++i)
                Create();
        }
        GUILayout.EndHorizontal();

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
        GUILayout.Space(50);
        GUI.color = Color.red;
        if (GUILayout.Button("Delete"))
            Delete();
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
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
        }

        GUILayout.EndHorizontal();

        _itemScrollPos = EditorGUILayout.BeginScrollView(_itemScrollPos, GUILayout.Width (position.width - ListWidth), GUILayout.Height(position.height - 70));

        if (_data.Contains(_selectedKey) && _data[_selectedKey].AsDictonary != null)
        {
            var origDict = _data[_selectedKey].AsDictonary;

            var assembly = Assembly.Load("Assembly-CSharp");
            var origType = assembly.GetTypes().Where(x => x.Name.Equals(target.name)).First();

            var origObj = JsonConverter.ToObject(origType, origDict.ToString());

            OnObjectGUI(origObj);

            var newDict = JsonConverter.ToJsonObject(origObj).AsDictonary;
            _data[_selectedKey] = newDict;

            // 멀티 셀렉트 된 데이터들에 현재 데이터에서 수정된 값만 반영.
            foreach (var dataKey in _multiSelectedKey)
            {
                foreach (var origPair in origDict)
                {
                    if (origPair.Value.ToString() != newDict[origPair.Key].ToString())
                    {
                        _data[dataKey].AsDictonary[origPair.Key] = newDict[origPair.Key];
                    }
                }
            }
        }

        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private object OnObjectGUI(object obj)
    {
        return OnObjectGUI(obj, null);
    }

    private object OnObjectGUI(object obj, string refName)
    {
        if (obj == null)
            return null;

        var type = obj.GetType();

        if (type == typeof(int))
        {
            obj = EditorGUILayout.IntField((int)obj);
        }
        else if (type == typeof(float))
        {
            obj = EditorGUILayout.FloatField((float)obj);
        }
        else if (type == typeof(string))
        {
//            obj = EditorGUILayout.TextField((string)obj);
            var available = refName != null && _refers[refName].Contains((string)obj);
            GUI.contentColor = available ? Color.green : Color.red;
            if (refName == null)
                GUI.contentColor = Color.white;
            
            var val = EditorGUILayout.TextField((string)obj);
            if (val == null)
                val = "";
            obj = val;
            GUI.contentColor = Color.white;
            if (refName != null)
            {
                var refs = _refers[refName].ToArray();
                _useRefs = GUILayout.Toggle(_useRefs, string.Format("({0})", refName));
                GUILayout.BeginVertical();
                if (_useRefs)
                {
                    GUI.contentColor = Color.cyan;
                    var refLen = refs.Length;
                    for (var refIdx = 0; refIdx < refLen; ++refIdx)
                    {
                        var r = refs[refIdx];
                        if (r.IndexOf(val) >= 0)
                            GUILayout.Label(string.Format("{0}{1}", r, _refNames[refName][refIdx]));
                    }
                    GUI.contentColor = Color.white;
                }
                GUILayout.EndVertical();
            }

        }
        else if (type == typeof(bool))
        {
            obj = EditorGUILayout.Toggle((bool)obj);
        }
        else if (type.IsEnum)
        {
            obj = EditorGUILayout.EnumPopup((Enum)obj);
        }
        else
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                GUILayout.BeginHorizontal();
                OnPropertyGUI(obj, property);
                GUILayout.EndHorizontal();
            }
        }
        return obj;
    }

    private void OnPropertyGUI(object obj, PropertyInfo property)
    {
        var labelWidth = GUILayout.Width(100);
        GUILayout.Label(property.Name, labelWidth);
        if (typeof(IList).IsAssignableFrom(property.PropertyType))
        {
            IList ilist = (IList)property.GetValue(obj, null);
            if (ilist == null)
                ilist = (IList)Activator.CreateInstance(property.PropertyType);
            property.SetValue(obj, ilist, null);
            
            GUILayout.BeginVertical();
            var count = EditorGUILayout.IntField("size", ilist.Count);
            Type listType = ilist.GetType().GetGenericArguments()[0];

            string refName = null;
            var attrs = property.GetCustomAttributes(true);
            foreach (var a in attrs)
            {
                var r = a as JsonStoreRefer;
                if (r != null)
                    refName = r.Name;
            }

            while (ilist.Count < count)
            {
                if (listType == typeof(string))
                {
                    ilist.Add("");
                }
                else
                    ilist.Add(Activator.CreateInstance(listType));
            }
            while (ilist.Count > count)
            {
                ilist.RemoveAt(ilist.Count - 1);
            }
            for (var i = 0; i < ilist.Count; ++i)
            {
                ilist[i] = OnObjectGUI(ilist[i], refName);
            }
            GUILayout.EndVertical();
        }
        else if (property.PropertyType == typeof(int))
        {
            property.SetValue(obj, EditorGUILayout.IntField((int)property.GetValue(obj, null)), null);
        }
        else if (property.PropertyType == typeof(float))
        {
            property.SetValue(obj, EditorGUILayout.FloatField((float)property.GetValue(obj, null)), null);
        }
        else if (property.PropertyType == typeof(string))
        {
            string refName = null;
            var attrs = property.GetCustomAttributes(true);
            foreach (var a in attrs)
            {
                var r = a as JsonStoreRefer;
                if (r != null)
                    refName = r.Name;
            }

            var available = refName != null && _refers[refName].Contains((string)property.GetValue(obj, null));
            GUI.contentColor = available ? Color.green : Color.red;
            if (refName == null)
                GUI.contentColor = Color.white;

            var val = EditorGUILayout.TextField((string)property.GetValue(obj, null));
            if (val == null)
                val = "";
            property.SetValue(obj, val, null);
            GUI.contentColor = Color.white;

            if (refName != null)
            {
                var refs = _refers[refName].ToArray();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                _useRefs = GUILayout.Toggle(_useRefs, string.Format("({0})", refName), labelWidth);
                GUILayout.BeginVertical();
                if (_useRefs)
                {
                    GUI.contentColor = Color.cyan;
                    var refLen = refs.Length;
                    for (var refIdx = 0; refIdx < refLen; ++refIdx)
                    {
                        var r = refs[refIdx];
                        if (r.IndexOf(val) >= 0)
                            GUILayout.Label(string.Format("{0}{1}", r, _refNames[refName][refIdx]));
                    }
                    GUI.contentColor = Color.white;
                }
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
            }
        }
        else if (property.PropertyType == typeof(bool))
        {
            var val = (bool)property.GetValue(obj, null);
            val = EditorGUILayout.Toggle(val);
            property.SetValue(obj, val, null);
        }
        else if (property.PropertyType.IsEnum)
        {
            var v = (System.Enum)property.GetValue(obj, null);
            v = EditorGUILayout.EnumPopup(v);
            property.SetValue(obj, v, null);
        }
        else if (property.PropertyType.IsClass)
        {
            var value = property.GetValue(obj, null);
            if (value == null)
                value = JsonConverter.ToObject(property.PropertyType, "{}");
            GUILayout.BeginVertical();
            OnObjectGUI(value);
            GUILayout.EndVertical();
            property.SetValue(obj, value, null);
        }
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
