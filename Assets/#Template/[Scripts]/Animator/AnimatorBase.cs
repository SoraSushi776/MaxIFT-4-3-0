using DancingLineFanmade.Level;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Animated
{
    public enum TransformType
    {
        New,
        Add
    }

    public enum AnimatorType
    {
        Position,
        Rotation,
        Scale
    }

    public class AnimatorBase : MonoBehaviour
    {
        private Transform selfTransform;

        [SerializeField] internal UnityEvent onAnimatorStart = new UnityEvent();
        [SerializeField] internal UnityEvent onAnimatorFinished = new UnityEvent();
        [SerializeField, EnumToggleButtons] protected TransformType transformType = TransformType.New;
        [SerializeField] protected bool triggeredByTime = true;
        [SerializeField, MinValue(0f)] protected float triggerTime = 0f;
        [SerializeField, MinValue(0f)] private float duration = 2f;
        [SerializeField] private bool offsetTime = false;
        [SerializeField] protected bool dontRevive = false;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] protected Vector3 originalTransform = Vector3.zero;

        protected bool finished = false;
        protected Vector3 finalTransform = Vector3.zero;
        protected int index;

        private void OnEnable()
        {
            selfTransform = transform;
        }

        protected void TriggerAnimator(AnimatorType type, RotateMode rotateMode = RotateMode.Fast)
        {
            finished = true;
            onAnimatorStart.Invoke();
            Animator(type, rotateMode).OnComplete(() => onAnimatorFinished.Invoke());
        }

        protected void InitTransform(AnimatorType type)
        {
            switch (type)
            {
                case AnimatorType.Position:
                    selfTransform.localPosition = originalTransform;
                    break;
                case AnimatorType.Rotation:
                    selfTransform.localEulerAngles = originalTransform;
                    break;
                case AnimatorType.Scale:
                    selfTransform.localScale = originalTransform;
                    break;
            }
        }

        protected void InitTime()
        {
            triggerTime = offsetTime ? triggerTime - duration : triggerTime;
        }

        private Tween Animator(AnimatorType type, RotateMode rotateMode)
        {
            index = Player.Instance.Checkpoints.Count;

            switch (type)
            {
                case AnimatorType.Position: return selfTransform.DOLocalMove(finalTransform, duration).SetEase(ease);
                case AnimatorType.Rotation: return selfTransform.DOLocalRotate(finalTransform, duration, rotateMode).SetEase(ease);
                case AnimatorType.Scale: return selfTransform.DOScale(finalTransform, duration).SetEase(ease);
                default: return null;
            }
        }
    }
}