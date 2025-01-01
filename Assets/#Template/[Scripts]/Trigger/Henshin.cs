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
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Player.Instance.henShin = enableHenshin;
            Player.Instance.henshinObject = henshinObject;
            Player.Instance.objectOffset = objectOffset;
            Player.Instance.showLineTail = showLineTail;
            Player.Instance.showLineBody = showLineBody;
        }
    }
}
