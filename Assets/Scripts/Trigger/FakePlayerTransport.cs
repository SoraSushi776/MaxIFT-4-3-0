using System;
using System.Collections;
using System.Collections.Generic;
using DancingLineFanmade.Level;
using UnityEngine;
using Sirenix.OdinInspector;

[DisallowMultipleComponent, RequireComponent(typeof(Collider))]
public class FakePlayerTransport : MonoBehaviour
{
    public FakePlayer fakePlayer;
    
    public bool tpToPlayer;
    [ShowIf("tpToPlayer")] public Vector3 offset;

    [ShowIf("@!tpToPlayer")]
    [EnumToggleButtons]
    public TransportType transportType;
    [ShowIf("@!tpToPlayer && transportType == TransportType.Transform")] public Transform target;
    [ShowIf("@!tpToPlayer && transportType == TransportType.Vector3")] public Vector3 position;

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (tpToPlayer)
            {
                fakePlayer.transform.localPosition = other.transform.localPosition + offset;
            }
            else
            {
                switch (transportType)
                {
                    case TransportType.Transform:
                        fakePlayer.transform.localPosition = target.localPosition;
                        break;
                    case TransportType.Vector3:
                        fakePlayer.transform.localPosition = position;
                        break;
                }
            }
        }
    }

    public enum TransportType
    {
        Transform,
        Vector3
    }
}
