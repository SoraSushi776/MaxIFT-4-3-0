using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [Serializable]
    public class SingleAnimator
    {
        public Animator animator;
        public bool dontRevive;

        private float progress;
        internal bool played;

        private bool playState;

        public void IntiAnimator()
        {
            animator.speed = 0f;
            played = false;
        }

        public void PlayAnimator()
        {
            animator.speed = 1f;
            played = true;
        }

        public void StopAnimator()
        {
            animator.speed = 0f;
        }

        public void SetState()
        {
            animator.Play(animator.GetCurrentAnimatorClipInfo(0)[0].clip.name, 0, progress);
            played = playState;
        }

        public void GetState()
        {
            progress = animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            playState = played;
        }
    }

    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class PlayAnimator : MonoBehaviour
    {
        [SerializeField, TableList] internal List<SingleAnimator> animators = new List<SingleAnimator>();

        private void Start()
        {
            foreach (SingleAnimator a in animators) a.IntiAnimator();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) foreach (SingleAnimator a in animators) if (!a.played) a.PlayAnimator();
        }
    }
}