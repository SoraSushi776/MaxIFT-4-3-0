using DancingLineFanmade.Level;
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !setFakePlayer)
            {
                Player.Instance.Speed = speed;
                if (setCameraFollowSpeed && CameraFollower.Instance) CameraFollower.Instance.followSpeed *= speed / 12f;
            }
            if ((other.CompareTag("FakePlayer") || other.CompareTag("Obstacle")) && setFakePlayer) player.speed = speed;
        }
    }
}