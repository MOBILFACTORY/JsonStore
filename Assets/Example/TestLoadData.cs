using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestLoadData : MonoBehaviour
{
    public JsonStore<TestObject> testObject = new JsonStore<TestObject>();

    private void Start()
    {
        testObject.Load(Resources.Load<TextAsset>("TestObject"));

        Debug.Log("Count: " + testObject.Count.ToString());
        var t = testObject["1"];
        Debug.Log(t.attackDamage);
    }
}
