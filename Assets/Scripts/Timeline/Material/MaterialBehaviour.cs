using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class MaterialBehaviour : PlayableBehaviour
{
    public Color TargetColor = Color.white;
    [ColorUsage(false,true)]public Color TargetHDRColor = Color.white;
}
