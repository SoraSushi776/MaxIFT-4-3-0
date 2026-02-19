using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class Jump : MonoBehaviour
    {
        [SerializeField, MinValue(0f)] internal float power = 500f;
        [SerializeField] private bool changeDirection = false;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (changeDirection) Player.Instance.Turn();
                Player.characterRigidbody.AddForce(0, power, 0, ForceMode.Impulse);
                Player.Instance.Events?.Invoke(7);
            }
        }

#if UNITY_EDITOR
        [Button("Add Predictor", ButtonSizes.Large), HideIf("@gameObject.GetComponent<TrailPredictor>() != null")]
        private void Add()
        {
            gameObject.AddComponent<TrailPredictor>();
        }
#endif
    }
}