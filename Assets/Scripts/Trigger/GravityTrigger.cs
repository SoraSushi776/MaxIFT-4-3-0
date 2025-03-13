using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class GravityTrigger : MonoBehaviour
    {
        [SerializeField] private Vector3 gravity = LevelManager.defaultGravity;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) Physics.gravity = gravity;
        }
    }
}