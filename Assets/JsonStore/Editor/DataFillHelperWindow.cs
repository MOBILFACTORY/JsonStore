﻿using UnityEngine;
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

        var assembly = Assembly.Load("Assembly-CSharp");
        var t = assembly.GetTypes().Where(x => x.Name.Equals(asset.name)).First();

        var obj = JsonConverter.ToObject(t, "{}");

        if (KeyStr == selectedFieldName)
            GUI.contentColor = Color.green;
        if (GUILayout.Button(KeyStr))
        {
            selectedFieldName = KeyStr;
            selectedPropName = "";
        }
        GUI.contentColor = Color.white;
        GUILayout.Space(10);

        DrawProperties(obj);
        
        GUILayout.EndVertical();
    }

    private void DrawProperties(object obj)
    {
        if (obj == null)
            return;

        foreach (var property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            var type = property.PropertyType;

            if (type == typeof(int)
                || type == typeof(long)
                || type == typeof(float)
                || type == typeof(double)
                || type == typeof(bool)
                || type == typeof(string))
            {
                if (property.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                if (GUILayout.Button(property.Name))
                {
                    selectedFieldName = property.Name;
                    selectedPropName = "";
                }
                GUI.contentColor = Color.white;
            }
            else if (typeof(IList).IsAssignableFrom(type) == true)
            {
                GUILayout.Space(10);
                if (property.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                GUILayout.Label(string.Format("{0} (Collection)", property.Name));
                GUI.contentColor = Color.white;
                var t = property.PropertyType.GetGenericArguments()[0];
                var o = Activator.CreateInstance(t);
                DrawProps(o, property.Name, true);
            }
            else if (typeof(IDictionary).IsAssignableFrom(type) == true)
            {
                GUILayout.Space(10);
                if (property.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                GUILayout.Label(string.Format("{0} (Collection)", property.Name));
                GUI.contentColor = Color.white;
                var t = property.PropertyType.GetGenericArguments()[1];
                var o = Activator.CreateInstance(t);
                DrawProps(o, property.Name, true);
            }
            else if (type.IsClass == true)
            {
                GUILayout.Space(10);
                if (property.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                GUILayout.Label(property.Name);
                GUI.contentColor = Color.white;
                var o = Activator.CreateInstance(property.PropertyType);
                DrawProps(o, property.Name, false);
            }
            else if (type.IsEnum == true)
            {
                if (property.Name == selectedFieldName)
                    GUI.contentColor = Color.green;
                if (GUILayout.Button(property.Name))
                {
                    selectedFieldName = property.Name;
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

        var newDict = new Dictionary<string, JsonObject>();
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
        var newKeys = new List<string>();
        foreach (var pair in newDict)
        {
            newKeys.Add(pair.Key);
        }
        newKeys.Reverse();
        foreach (var key in newKeys)
        {
            jsonDict.Add(key, newDict[key]);
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

            var assembly = Assembly.Load("Assembly-CSharp");
            var origType = assembly.GetTypes().Where(x => x.Name.Equals(asset.name)).First();

            var obj = JsonConverter.ToObject(origType, "{}");
            var property = obj.GetType().GetProperty(selectedFieldName);
            JsonObject jsonObj = new JsonString(valArr[valIdx], true);
            if (property.PropertyType == typeof(int))
            {
                jsonObj = new JsonNumber(int.Parse(valArr[valIdx]));
            }
            else if (property.PropertyType == typeof(long))
            {
                jsonObj = new JsonNumber(long.Parse(valArr[valIdx]));
            }
            else if (property.PropertyType == typeof(float))
            {
                jsonObj = new JsonNumber(float.Parse(valArr[valIdx]));
            }
            else if (property.PropertyType == typeof(double))
            {
                jsonObj = new JsonNumber(double.Parse(valArr[valIdx]));
            }
            else if (property.PropertyType == typeof(bool))
            {
                jsonObj = new JsonBool(bool.Parse(valArr[valIdx]));
            }
            else if (property.PropertyType == typeof(string))
            {
                jsonObj = new JsonString(valArr[valIdx], false);
            }
            else if (property.PropertyType.IsClass)
            {
                var o = JsonConverter.ToObject(property.PropertyType, jsonDict[key].AsDictonary[selectedFieldName].ToString());
                var p = o.GetType().GetProperty(selectedPropName);
                p.SetValue(o, Convert.ChangeType((object)valArr[valIdx], p.PropertyType), null);
                jsonObj = JsonConverter.ToJsonObject(o);
            }
            else if (property.PropertyType.IsEnum)
            {
                jsonObj = new JsonNumber(int.Parse(valArr[valIdx]));
            }
            
            jsonDict[key].AsDictonary[selectedFieldName] = jsonObj;
            valIdx++;
        }
    }
}
