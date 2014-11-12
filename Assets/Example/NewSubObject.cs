using UnityEngine;

public class NewSubObject
{
    public int i;
    public float f;
    public string s;
    [JsonStoreRefer("ReferObject")]
    public string r;
}