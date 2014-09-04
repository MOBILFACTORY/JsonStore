using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestLoadData
{
    public JsonStore<TestObject> testObject = new JsonStore<TestObject>();

    public void Load()
    {
        testObject.Load(Resources.Load<TextAsset>("..."));
    }
}
