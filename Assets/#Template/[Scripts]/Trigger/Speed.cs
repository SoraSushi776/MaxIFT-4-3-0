using DancingLineFanmade.Level;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class Speed : MonoBehaviour
    {
        [SerializeField] private bool setFakePlayer = false;
        [SerializeField, ShowIf("setFakePlayer")] private FakePlayer player;
        [SerializeField, MinValue(0)] private int speed = 12;
        [SerializeField, HideIf("setFakePlayer")] private bool setCameraFollowSpeed = true;
        [SerializeField, ShowIf("setCameraFollowSpeed")] private Vector3 speedCam = new(1.2f, 3f, 6f);
        [SerializeField, ShowIf("setCameraFollowSpeed")] private float duration = 0.1f;
        [SerializeField, ShowIf("setCameraFollowSpeed")] private Ease ease = Ease.Linear;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !setFakePlayer)
            {
                Player.Instance.Speed = speed;
                if (setCameraFollowSpeed && CameraFollower.Instance) CameraFollower.Instance.SetFollowSpeed(speedCam, duration, ease);
            }
            if ((other.CompareTag("FakePlayer") || other.CompareTag("Obstacle")) && setFakePlayer) player.speed = speed;
        }
    }
}