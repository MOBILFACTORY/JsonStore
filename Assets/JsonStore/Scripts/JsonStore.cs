using UnityEngine;
using SolJSON.Convert;
using SolJSON.Types;
using System;
using System.Collections.Generic;

public class JsonStore<T> : Dictionary<string, T>
{
    public void Load(TextAsset json)
    {
        Load(json.name, json.text);
    }

    public void Load(string objName, string jsonStr)
    {
        var jsonDict = JsonConverter.ToJsonObject(jsonStr).AsDictonary;
        foreach (var pair in jsonDict)
        {
            var o = JsonConverter.ToObject<T>(pair.Value);
            this.Add(pair.Key, o);
        }
    }
}

[AttributeUsage(AttributeTargets.Field)]
public class JsonStoreRefer : Attribute
{
    public string _name;

    public JsonStoreRefer(string name)
    {
        _name = name;
    }

    public string Name
    {
        get
        {
            return _name;
        }
    }
}