using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class CameraShakeTrigger : MonoBehaviour
    {
        [SerializeField] private float power = 1f;
        [SerializeField] private float duration = 2f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && CameraFollower.Instance) CameraFollower.Instance.DoShake(power, duration);
        }
    }
}