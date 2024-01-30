using DG.Tweening;
using UnityEngine;

namespace DancingLineFanmade.Level
{
    public static class AudioManager
    {
        public static void PlayClip(AudioClip clip, float volume)
        {
            AudioSource audioSource = new GameObject("One shot sound: " + clip.name).AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
            Object.Destroy(audioSource.gameObject, clip.length);
        }

        public static AudioSource PlayTrack(AudioClip clip, float volume)
        {
            AudioSource audioSource = new GameObject(clip.name).AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.Play();
            return audioSource;
        }

        public static float Time
        {
            get => Player.Instance.SoundTrack.time;
            set => Player.Instance.SoundTrack.time = value;
        }

        public static float Pitch
        {
            get => Player.Instance.SoundTrack.pitch;
            set => Player.Instance.SoundTrack.pitch = value;
        }

        public static float Volume
        {
            get => Player.Instance.SoundTrack.volume;
            set => Player.Instance.SoundTrack.volume = value;
        }

        public static float Progress
        {
            get => Player.Instance.SoundTrack.time / Player.Instance.SoundTrack.clip.length;
        }

        public static void Stop()
        {
            Player.Instance.SoundTrack.Stop();
        }

        public static void Play()
        {
            Player.Instance.SoundTrack.Play();
        }

        public static Tween FadeOut(float volume, float duration)
        {
            return Player.Instance.SoundTrack.DOFade(volume, duration).SetEase(Ease.Linear).OnComplete(new TweenCallback(Stop));
        }
    }
}