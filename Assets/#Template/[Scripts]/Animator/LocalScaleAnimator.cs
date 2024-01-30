using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Animated
{
    public class LocalScaleAnimator : AnimatorBase
    {
        [SerializeField] private Vector3 scale = Vector3.one;

        private void Start()
        {
            switch (transformType)
            {
                case TransformType.New: finalTransform = scale; break;
                case TransformType.Add: finalTransform = originalTransform + scale; break;
            }
            InitTransform(AnimatorType.Scale);
            if (triggeredByTime) InitTime();
        }

        private void Update()
        {
            if (!finished && LevelManager.GameState == GameStatus.Playing && AudioManager.Time > triggerTime && triggeredByTime) Trigger();
        }

        public void Trigger()
        {
            TriggerAnimator(AnimatorType.Scale);
            if (!dontRevive) LevelManager.revivePlayer += ResetData;
        }

        private void ResetData()
        {
            LevelManager.revivePlayer -= ResetData;
            LevelManager.CompareCheckpointIndex(index, () =>
            {
                InitTransform(AnimatorType.Scale);
                finished = false;
            });
        }

        private void OnDestroy()
        {
            LevelManager.revivePlayer -= ResetData;
        }

#if UNITY_EDITOR
        [Button("Get original scale", ButtonSizes.Large), HorizontalGroup("0")]
        private void GetOriginalScale()
        {
            originalTransform = transform.localScale;
        }

        [Button("Set as original scale", ButtonSizes.Large), HorizontalGroup("0")]
        private void SetOriginalScale()
        {
            transform.localScale = originalTransform;
        }

        [Button("Get new scale", ButtonSizes.Large), HorizontalGroup("1")]
        private void GetNewScale()
        {
            switch (transformType)
            {
                case TransformType.New:
                    scale = transform.localScale;
                    break;
                case TransformType.Add:
                    scale = transform.localScale - originalTransform;
                    break;
            }
        }

        [Button("Set as new scale", ButtonSizes.Large), HorizontalGroup("1")]
        private void SetNewScale()
        {
            switch (transformType)
            {
                case TransformType.New:
                    transform.localScale = scale;
                    break;
                case TransformType.Add:
                    transform.localScale = originalTransform + scale;
                    break;
            }
        }
#endif
    }
}