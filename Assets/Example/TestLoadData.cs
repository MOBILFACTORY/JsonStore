using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestLoadData : MonoBehaviour
{
    public JsonStore<NewObject> store = new JsonStore<NewObject>();

    private void Start()
    {
        store.Load(Resources.Load<TextAsset>("NewObject").text);

        Debug.Log(string.Format("Count: {0}", store.Count));
        foreach (var i in store)
        {
            Debug.Log(i.ToString());
        }
    }
}
