using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class MaterialMixerBehaviour : PlayableBehaviour
{
    Color m_DefaultColor;
    Color m_AssignedColor;

    Color m_DefaultHDRColor;
    Color m_AssignedHDRColor;

    Material m_TrackBinding;
    bool m_FirstFrameHappened;

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        m_TrackBinding = playerData as Material;

        if (m_TrackBinding == null)
            return;

        if (!m_FirstFrameHappened)
        {
            m_DefaultColor = m_TrackBinding.color;
            m_DefaultHDRColor = m_TrackBinding.GetColor("_EmissionColor");
            m_FirstFrameHappened = true;
        }

        int inputCount = playable.GetInputCount ();

        Color blendedColor = Color.clear;
        Color blendedHDRColor = Color.clear;
        float totalWeight = 0f;
        float greatestWeight = 0f;
        int currentInputs = 0;

        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<MaterialBehaviour> inputPlayable = (ScriptPlayable<MaterialBehaviour>)playable.GetInput(i);
            MaterialBehaviour input = inputPlayable.GetBehaviour ();

            blendedColor += input.TargetColor * inputWeight;
            blendedHDRColor += input.TargetHDRColor * inputWeight;
            totalWeight += inputWeight;

            if (inputWeight > greatestWeight)
            {
                greatestWeight = inputWeight;
            }

            if (!Mathf.Approximately (inputWeight, 0f))
                currentInputs++;
        }

        m_TrackBinding.color = blendedColor + m_DefaultColor * (1f - totalWeight);
        m_TrackBinding.EnableKeyword("_EMISSION");
        m_TrackBinding.SetColor("_EmissionColor", blendedHDRColor + m_DefaultHDRColor * (1f - totalWeight));
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        m_FirstFrameHappened = false;

        if (m_TrackBinding == null)
            return;

        m_TrackBinding.color = m_DefaultColor;
        m_TrackBinding.EnableKeyword("_EMISSION");
        m_TrackBinding.SetColor("_EmissionColor", m_DefaultHDRColor);
    }
}
