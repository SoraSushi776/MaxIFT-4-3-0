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

            // 设置45°旋转
            transform.localEulerAngles = new Vector3(0, -45, 0);

            // 设置触发器大小
            transform.localScale = new Vector3(0.1f, 3, 10);
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("Player") && !triggered)
            {
                triggered = true;
                Player.Instance.Turn();
            }
        }
    }
}