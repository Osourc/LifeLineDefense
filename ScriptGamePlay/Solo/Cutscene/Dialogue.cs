using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Dialogue {

    public string type;
    public string name;
    public Sprite thumbnail;
    [TextArea(3,50)]
    public string[] sentences;
}