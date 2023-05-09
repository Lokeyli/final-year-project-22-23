using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://docs.unity3d.com/cn/current/Manual/class-ScriptableObject.html

[CreateAssetMenu(fileName = "Avatars", menuName = "ScriptableObjects/Avatars", order = 1)]
public class AvatarsScriptableObject : ScriptableObject
{
    public GameObject[] avatars;
}
