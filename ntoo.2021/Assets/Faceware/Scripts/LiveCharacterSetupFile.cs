using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LiveCharacterSetupFile : ScriptableObject {
    
    //[System.NonSerialized]
    public string Application = "Live";
    //[System.NonSerialized]
    public string Version = "1.0";

    [SerializeField]
    public List<string> Controls = new List<string>();
    [SerializeField]
    public List<Expression> Expressions = new List<Expression>();
    [SerializeField]
    public GameObject rootObject;
    [SerializeField]
    public string rootObjectName;
}
