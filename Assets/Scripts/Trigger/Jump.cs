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
                Player.Rigidbody.AddForce(0, power * Player.Rigidbody.mass, 0, ForceMode.Force);
                Player.Instance.Events?.Invoke(7);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (GetComponent<JumpPredictor>() && GetComponent<LineRenderer>()) GetComponent<JumpPredictor>().Draw();
        }

        [Button("Add Predictor", ButtonSizes.Large)]
        private void Add()
        {
            gameObject.AddComponent<JumpPredictor>();
        }
#endif
    }
}