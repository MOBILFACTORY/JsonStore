using SolJSON.Convert;
using SolJSON.Types;
using System;
using System.Collections.Generic;

public class JsonStore<T> : Dictionary<string, T>
{
    public void Load(string jsonStr)
    {
        var jsonDict = JsonConverter.ToJsonObject(jsonStr).AsDictonary;
        foreach (var pair in jsonDict)
        {
            var o = JsonConverter.ToObject<T>(pair.Value);
            this.Add(pair.Key, o);
        }
    }
}

[AttributeUsage(AttributeTargets.Property)]
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