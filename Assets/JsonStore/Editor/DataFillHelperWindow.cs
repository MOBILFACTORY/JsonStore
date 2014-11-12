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

public class DataFillHelper : EditorWindow
{
    private readonly string KeyStr = "#Key";

    private TextAsset asset;
    private JsonDictonary jsonDict;
    private string text = "";
    private string selectedKey = "";
    private string selectedFieldName = "";
    private string selectedPropName = "";

    public void SetData(TextAsset asset, JsonDictonary jsonDict, string selectedKey)
    {
        this.asset = asset;
        this.jsonDict = jsonDict;
        this.selectedKey = selectedKey;

        if (selectedKey == "")
        {
            foreach (var pair in jsonDict)
            {
                selectedKey = pair.Key;
                break;
            }
        }
    }

    
    void OnGUI()
    {
        DrawToolbar();
        DrawList();
        DrawTextArea();
    }

    private void Commit()
    {
        if (selectedFieldName == "")
        {
            EditorUtility.DisplayDialog("Error", "Select Field", "ok");
            return;
        }
        if (EditorUtility.DisplayDialog("Fill Data", "Fill Data", "ok", "cancel"))
        {
            Save();
            Close();
        }
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
        
        GUILayout.FlexibleSpace();
        
        if (GUILayout.Button("Commit", EditorStyles.toolbarButton))
            Commit();
        
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
    }

    private void DrawList()
    {
        if (asset == null)
        {
            Close();
            return;
        }
        
        GUILayout.BeginVertical(GUILayout.Width(140));

        var origType = Activator.CreateInstance("Assembly-CSharp", asset.name).Unwrap().GetType();
        var obj = JsonConverter.ToObject(origType, "{}");

        if (KeyStr == selectedFieldName)
            GUI.contentColor = Color.green;
        if (GUILayout.Button(KeyStr))
        {
            selectedFieldName = KeyStr;
            selectedPropName = "";
        }
        GUI.contentColor = Color.white;
        GUILayout.Space(10);

        DrawFields(obj);
        
        GUILayout.EndVertical();
    }

    private void DrawFields(object obj)
    {
        if (obj == null)
            return;

        foreach (var f in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            var type = f.FieldType;

            if (type == typeof(int)
                || type == typeof(long)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(bool)
                || type == typeof(string))
            {
                if (f.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                if (GUILayout.Button(f.Name))
                {
                    selectedFieldName = f.Name;
                    selectedPropName = "";
                }
                GUI.contentColor = Color.white;
            }
            else if (typeof(IList).IsAssignableFrom(type) == true)
            {
                GUILayout.Space(10);
                if (f.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                GUILayout.Label(string.Format("{0} (Collection)", f.Name));
                GUI.contentColor = Color.white;
                var t = f.FieldType.GetGenericArguments()[0];
                var o = Activator.CreateInstance(t);
                DrawProps(o, f.Name, true);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type) == true)
            {
                GUILayout.Space(10);
                if (f.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                GUILayout.Label(string.Format("{0} (Collection)", f.Name));
                GUI.contentColor = Color.white;
                var t = f.FieldType.GetGenericArguments()[1];
                var o = Activator.CreateInstance(t);
                DrawProps(o, f.Name, true);
            }
            else if (type.IsClass == true)
            {
                GUILayout.Space(10);
                if (f.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                GUILayout.Label(f.Name);
                GUI.contentColor = Color.white;
                var o = Activator.CreateInstance(f.FieldType);
                DrawProps(o, f.Name, false);
            }
            else if (type.IsEnum == true)
            {
                if (f.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                if (GUILayout.Button(f.Name))
                {
                    selectedFieldName = f.Name;
                    selectedPropName = "";
                }
                GUI.contentColor = Color.white;
            }
        }
    }

    public void DrawProps(object obj, string fieldName, bool collection)
    {
        if (obj == null)
            return;
        
        foreach (var p in obj.GetType().GetProperties())
        {
            var type = p.PropertyType;
            
            if (type == typeof(int)
                || type == typeof(long)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(bool)
                || type == typeof(string))
            {
                if (p.Name == selectedPropName)
                    GUI.contentColor = Color.green;
                if (collection)
                {
                    GUILayout.Label(string.Format("- {0}", p.Name));
                }
                else
                {
                    if (GUILayout.Button(p.Name))
                    {
                        selectedPropName = p.Name;
                        selectedFieldName = fieldName;
                    }
                }
                GUI.contentColor = Color.white;
            }
        }
    }

    private void DrawTextArea()
    {
        var left = 150;
        var top = 20;
        var rect = new Rect(left, top, position.width - left, position.height - top);
        text = EditorGUI.TextArea(rect, text);
    }

    private void Save()
    {
        if (selectedFieldName == KeyStr)
        {
            SaveKey();
        }
        else
        {
            SaveData();
        }
    }

    private void SaveKey()
    {
        var origDict = jsonDict;
        var origKeys = new List<string>();
        foreach (var pair in origDict)
        {
            origKeys.Add(pair.Key);
        }

        var newDict = new JsonDictonary();
        var valArr = text.Split('\n');
        var valIdx = 0;
        bool begin = false;
        foreach (var key in origKeys)
        {
            if (!begin)
            {
                if (key != selectedKey)
                    continue;
            }
            
            begin = true;

            if (valArr.Length == valIdx || valArr[valIdx] == "")
                break;

            newDict[valArr[valIdx]] = origDict[key];
            valIdx++;
        }

        foreach (var key in origKeys)
        {
            jsonDict.Remove(key);
        }
        foreach (var pair in newDict)
        {
            jsonDict.Add(pair.Key, pair.Value);
        }
    }

    private void SaveData()
    {
        var keys = new List<string>();
        foreach (var pair in jsonDict)
        {
            keys.Add(pair.Key);
        }
        
        var valArr = text.Split('\n');
        var valIdx = 0;
        bool begin = false;
        foreach (var key in keys)
        {
            if (!begin)
            {
                if (key != selectedKey)
                    continue;
            }
            
            begin = true;
            
            if (valArr.Length == valIdx || valArr[valIdx] == "")
                break;

            
            var origType = Activator.CreateInstance("Assembly-CSharp", asset.name).Unwrap().GetType();
            var obj = JsonConverter.ToObject(origType, "{}");
            var field = obj.GetType().GetField(selectedFieldName);
            JsonObject jsonObj = new JsonString(valArr[valIdx], true);
            if (field.FieldType == typeof(int))
            {
                jsonObj = new JsonNumber(int.Parse(valArr[valIdx]));
            }
            else if (field.FieldType == typeof(long))
            {
                jsonObj = new JsonNumber(long.Parse(valArr[valIdx]));
            }
            else if (field.FieldType == typeof(float))
            {
                jsonObj = new JsonNumber(float.Parse(valArr[valIdx]));
            }
            else if (field.FieldType == typeof(double))
            {
                jsonObj = new JsonNumber(double.Parse(valArr[valIdx]));
            }
            else if (field.FieldType == typeof(bool))
            {
                jsonObj = new JsonBool(bool.Parse(valArr[valIdx]));
            }
            else if (field.FieldType == typeof(string))
            {
                jsonObj = new JsonString(valArr[valIdx], false);
            }
            else if (field.FieldType.IsClass)
            {
                var o = JsonConverter.ToObject(field.FieldType, jsonDict[key].AsDictonary[selectedFieldName].ToString());
                var p = o.GetType().GetProperty(selectedPropName);
                p.SetValue(o, Convert.ChangeType((object)valArr[valIdx], p.PropertyType), null);
                jsonObj = JsonConverter.ToJsonObject(o);
            }
            else if (field.FieldType.IsEnum)
            {
                jsonObj = new JsonNumber(int.Parse(valArr[valIdx]));
            }
            
            jsonDict[key].AsDictonary[selectedFieldName] = jsonObj;
            valIdx++;
        }
    }
}
