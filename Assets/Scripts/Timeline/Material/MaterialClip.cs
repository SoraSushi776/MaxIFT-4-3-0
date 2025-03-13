using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class MaterialClip : PlayableAsset, ITimelineClipAsset
{
    public MaterialBehaviour template = new MaterialBehaviour ();

    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }

    public override Playable CreatePlayable (PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<MaterialBehaviour>.Create (graph, template);
        return playable;
    }
}
