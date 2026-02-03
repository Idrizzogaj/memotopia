using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName ="New Achievement", menuName = "Achievement")]
public class Achievement : ScriptableObject
{
    public Sprite icon;
    public string title;
    public string description;
    public string constantString;
}
