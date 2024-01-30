using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Auto
{
    [DisallowMultipleComponent]
    public class AutoPlay : MonoBehaviour
    {
        private Transform playerTransform;
        private Transform selfTransform;
        private float triggerDistance = 0.33f;
        private bool triggered = false;

        private float Distance
        {
            get => (selfTransform.position - playerTransform.position).sqrMagnitude;
        }

        private void Start()
        {
            selfTransform = transform;
            playerTransform = Player.Instance.transform;
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && Distance <= triggerDistance && !triggered)
            {
                triggered = true;
                Player.Instance.Turn();
            }
        }
    }
}