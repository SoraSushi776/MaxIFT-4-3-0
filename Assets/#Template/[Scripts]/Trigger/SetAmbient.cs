using DG.Tweening;
using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class SetAmbient : MonoBehaviour
    {
        [SerializeField] private AmbientSettings ambient;
        [SerializeField] private float duration = 2f;
        [SerializeField] private Ease ease = Ease.Linear;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) ambient.SetAmbient(duration, ease);
        }
    }
}