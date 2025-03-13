using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;

[TrackColor(0.4386792f, 0.7193396f, 1f)]
[TrackClipType(typeof(MaterialClip))]
[TrackBindingType(typeof(Material))]
public class MaterialTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        return ScriptPlayable<MaterialMixerBehaviour>.Create (graph, inputCount);
    }

    public override void GatherProperties (PlayableDirector director, IPropertyCollector driver)
    {
        base.GatherProperties (director, driver);
    }
}
