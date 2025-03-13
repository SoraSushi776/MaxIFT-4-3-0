using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent]
    public class PlayAudioClip : MonoBehaviour
    {
        [SerializeField] private AudioClip clip;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool triggeredByTrigger = true;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && triggeredByTrigger) PlayClip();
        }

        public void PlayClip()
        {
            AudioManager.PlayClip(clip, volume);
        }
    }
}