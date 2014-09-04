using UnityEngine;
using System.Collections;

[System.Serializable]
public class TestSubObject
{
    [SerializeField]
    private string _target;

    public string target
    {
        get { return _target; }
        set { _target = value; }
    }
}
