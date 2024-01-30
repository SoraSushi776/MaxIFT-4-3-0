using DancingLineFanmade.Level;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Trigger
{
    public class CameraTrigger : MonoBehaviour
    {
        private CameraFollower follower;

        [SerializeField] private UnityEvent onFinished = new UnityEvent();
        [SerializeField] private Vector3 offset = Vector3.zero;
        [SerializeField] private Vector3 rotation = new Vector3(54f, 45f, 0f);
        [SerializeField] private Vector3 scale = Vector3.one;
        [SerializeField, Range(0f, 179f)] private float fieldOfView = 80f;
        [SerializeField] private bool follow = true;
        [SerializeField, MinValue(0f)] private float duration = 2f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private RotateMode mode = RotateMode.FastBeyond360;
        [SerializeField] private bool canBeTriggered = true;

        private void Start()
        {
            follower = CameraFollower.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && canBeTriggered)
            {
                follower.follow = follow;
                follower.Trigger(offset, rotation, scale, fieldOfView, duration, ease, mode, onFinished);
            }
        }

        public void Trigger()
        {
            if (!canBeTriggered)
            {
                follower.follow = follow;
                follower.Trigger(offset, rotation, scale, fieldOfView, duration, ease, mode, onFinished);
            }
        }
    }
}