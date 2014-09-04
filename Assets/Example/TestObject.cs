using UnityEngine;
using System.Collections;

public class TestObject : ScriptableObject
{
    public new string name;
    public int hp;
    public int attackDamage;
    public float speed;
    public TestSubObject sub;
}
