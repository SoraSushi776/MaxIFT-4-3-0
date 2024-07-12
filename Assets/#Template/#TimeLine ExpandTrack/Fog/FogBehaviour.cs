using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Playables;

[Serializable]
public class FogBehaviour : PlayableBehaviour
{
    public FogMode TargetFogMode = FogMode.Linear;
    [ColorUsage(false)]public Color TargetFogColor = Color.white;
    [ShowIf("IsLinear")]public float FogStartDistance, FogEndDistance;
    [HideIf("IsLinear")]public float FogDensity;

    private bool IsLinear => TargetFogMode == FogMode.Linear;
}