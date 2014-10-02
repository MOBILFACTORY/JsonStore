using UnityEngine;
using SolJSON.Convert;
using SolJSON.Convert.Helper;
using SolJSON.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class JsonStore<T> : Dictionary<string, T> where T : ScriptableObject
{
    public void Load(TextAsset json)
    {
        var t = ScriptableObject.CreateInstance(typeof(T));
        Load(json.name, json.text);
    }

    public void Load(string objName, string jsonStr)
    {
        var jsonDict = SolJSON.Convert.JsonConverter.ToJsonObject(jsonStr).AsDictonary;
        foreach (var pair in jsonDict)
        {
            var o = (T)JsonStore.ToScriptableObject(objName, pair.Value.AsDictonary);
            this.Add(pair.Key, o);
        }
    }
}

public class JsonStore
{
    public static ScriptableObject ToScriptableObject(string name, JsonDictonary dict)
    {
        ScriptableObject obj = ScriptableObject.CreateInstance(name);
        Type type = obj.GetType();
        foreach (var pair in dict)
        {
            FieldInfo f = type.GetField(pair.Key);
            if (f == null)
                continue;
            
            f.SetValue(obj, JsonObjectToObject.Convert(f.FieldType, pair.Value));
        }
        return obj;
    }
    
    public static JsonObject ToJsonObject(ScriptableObject obj)
    {
        Type type = obj.GetType();
        JsonDictonary dict = new JsonDictonary();
        foreach (var f in type.GetFields())
        {
            dict.Add(f.Name, ObjectToJsonObject.Convert(f.GetValue(obj)));
        }
        return dict;
    }
}