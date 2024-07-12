using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class FogMixerBehaviour : PlayableBehaviour
{
    Color Default_FogColor;
    Color Assigned_FogColor;
    float Default_FogStartDistance;
    float Assigned_FogStartDistance;
    float Default_FogEndDistance;
    float Assigned_FogEndDistance;
    float Default_FogDensity;
    float Assigned_FogDensity;

    bool m_FirstFrameHappened;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!m_FirstFrameHappened)
        {
            Default_FogColor = RenderSettings.fogColor;
            Default_FogStartDistance = RenderSettings.fogStartDistance;
            Default_FogEndDistance = RenderSettings.fogEndDistance;
            Default_FogDensity = RenderSettings.fogDensity;
            m_FirstFrameHappened = true;
        }

        int inputCount = playable.GetInputCount ();

        FogMode fogMode = FogMode.Linear;
        Color blendedFogColor = Color.clear;
        float blendedFogStartDistance = 0f;
        float blendedFogEndDistance = 0f;
        float blendedFogDensity = 0f;

        float totalWeight = 0f;
        float greatestWeight = 0f;
        int currentInputs = 0;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<FogBehaviour> inputPlayable = (ScriptPlayable<FogBehaviour>)playable.GetInput(i);
            FogBehaviour input = inputPlayable.GetBehaviour ();

            fogMode = input.TargetFogMode;
            blendedFogColor += input.TargetFogColor * inputWeight;
            if (fogMode == FogMode.Linear)
            {
                blendedFogStartDistance += input.FogStartDistance * inputWeight;
                blendedFogEndDistance += input.FogEndDistance * inputWeight;
            }
            else
            {
                blendedFogDensity += input.FogDensity * inputWeight;
            }
            totalWeight += inputWeight;

            if (inputWeight > greatestWeight)
            {
                greatestWeight = inputWeight;
            }

            if (!Mathf.Approximately (inputWeight, 0f))
                currentInputs++;
        }

        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = blendedFogColor + Default_FogColor * (1f - totalWeight);
        if (fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = blendedFogStartDistance + Default_FogStartDistance * (1f - totalWeight);
            RenderSettings.fogEndDistance = blendedFogEndDistance + Default_FogEndDistance * (1f - totalWeight);
        }
        else
        {
            RenderSettings.fogDensity = blendedFogDensity + Default_FogDensity * (1f - totalWeight);
        }
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        m_FirstFrameHappened = false;

        RenderSettings.fogColor = Default_FogColor;
        RenderSettings.fogStartDistance = Default_FogStartDistance;
        RenderSettings.fogEndDistance = Default_FogEndDistance;
        RenderSettings.fogDensity = Default_FogDensity;
    }
}
