using System;
using System.Collections;
using System.Collections.Generic;
using DancingLineFanmade.Level;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

public class Henshin : MonoBehaviour
{
    public bool enableHenshin;

    [ShowIf("@enableHenshin")] public Transform henshinObject;
    [ShowIf("@enableHenshin")] public Vector3 objectOffset;
    [ShowIf("@enableHenshin")] public bool showLineTail, showLineBody;
    [ShowIf("@enableHenshin")] public float animationTime;
    public Facing facing;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.henShin = enableHenshin;
            Player.Instance.henshinObject = henshinObject;
            Player.Instance.objectOffset = objectOffset;
            Player.Instance.showLineTail = showLineTail;
            Player.Instance.showLineBody = showLineBody;
            Player.Instance.rotationTime = animationTime;

            if (facing == Facing.FirstDirection)
                Player.Instance.henshinObject.transform.eulerAngles = Player.Instance.firstDirection;
            else if (facing == Facing.SecondDirection)
                Player.Instance.henshinObject.transform.eulerAngles = Player.Instance.secondDirection;
        }
    }

    public enum Facing
    {
        DontChange,
        FirstDirection,
        SecondDirection
    }
}
