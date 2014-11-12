using UnityEngine;
using System.Collections.Generic;

public enum NewEnum
{
    A,
    B,
    C
}

public class NewObject
{
    public string name;
    public NewEnum e;
    public int i;
    public float f;
    public string s;
    [JsonStoreRefer("ReferObject")]
    public string r;
    public NewSubObject sub;
    public List<int> i_list;
}