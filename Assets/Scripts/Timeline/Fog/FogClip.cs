using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class FogClip : PlayableAsset, ITimelineClipAsset
{
    public FogBehaviour template = new FogBehaviour();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<FogBehaviour>.Create (graph, template);
        return playable;
    }
}
