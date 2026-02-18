using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Level
{
    [DisallowMultipleComponent]
    public class CameraFollower : MonoBehaviour
    {
        private Transform selfTransform;

        public static CameraFollower Instance { get; private set; }

        public Transform target;
        public Transform rotator;
        public Transform scale;
        public Vector3 defaultOffset = Vector3.zero;
        public Vector3 defaultRotation = new(60f, 45f, 0f);
        public Vector3 defaultScale = Vector3.one;
        public bool follow = true;
        public bool smooth = true;

        public Camera FollowingCamera { get; set; }
        private Player Player { get; set; }
        private Tween OffsetTween { get; set; }
        private Tween RotationTween { get; set; }
        private Tween ScaleTween { get; set; }
        private Tween ShakeTween { get; set; }
        private Tween FovTween { get; set; }
        private float ShakePower { get; set; }
        private Quaternion Rotation { get; set; }
        public Vector3 FollowSpeed = new(1.2f, 3f, 6f);

        private Vector3 Translation
        {
            get
            {
                var targetPosition = Rotation * target.position;
                var selfPosition = Rotation * selfTransform.position;
                return targetPosition - selfPosition;
            }
        }

        private Transform Origin { get; set; }

        Tween _followspeed;

        public void SetFollowSpeed(Vector3 target, float duration, Ease ease)
        {
            _followspeed?.Kill(false);
            _followspeed = DOTween.To(()=> FollowSpeed, a => FollowSpeed = a, target, duration);
            _followspeed.SetEase(ease);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            selfTransform = transform;
            Player = Player.Instance;
            Rotation = Quaternion.Euler(GetRotatingVector(Player.firstDirection, Player.secondDirection, false));
            FollowingCamera = Player.sceneCamera;
            SetDefaultTransform();
            Origin = new GameObject("CameraMovementOrigin")
            {
                transform =
                {
                    position = Vector3.zero,
                    rotation = Quaternion.Euler(GetRotatingVector(Player.firstDirection, Player.secondDirection, true)),
                    localScale = Vector3.one
                }
            }.transform;
        }

        private void Update()
        {
            var translation = new Vector3(Translation.x * Time.smoothDeltaTime * FollowSpeed.x,
                Translation.y * Time.smoothDeltaTime * FollowSpeed.y,
                Translation.z * Time.smoothDeltaTime * FollowSpeed.z);
            if (LevelManager.GameState == GameStatus.Playing && follow)
                selfTransform.Translate(smooth ? translation : Translation, Origin);
        }

        public void Trigger(Vector3 n_offset, Vector3 n_rotation, Vector3 n_scale, float n_fov, float duration,
            Ease ease, RotateMode mode, UnityEvent callback, bool use, AnimationCurve curve)
        {
            SetOffset(n_offset, duration, ease, use, curve);
            SetRotation(n_rotation, duration, mode, ease, use, curve);
            SetScale(n_scale, duration, ease, use, curve);
            SetFov(n_fov, duration, ease, use, curve);
            RotationTween.OnComplete(callback.Invoke);
        }

        public void KillAll()
        {
            OffsetTween?.Kill();
            RotationTween?.Kill();
            ScaleTween?.Kill();
            ShakeTween?.Kill();
            FovTween?.Kill();
        }

        private void SetOffset(Vector3 n_offset, float duration, Ease ease, bool use, AnimationCurve curve)
        {
            if (OffsetTween != null)
            {
                OffsetTween.Kill();
                OffsetTween = null;
            }

            OffsetTween = !use
                ? rotator.DOLocalMove(n_offset, duration).SetEase(ease)
                : rotator.DOLocalMove(n_offset, duration).SetEase(curve);
        }

        private void SetRotation(Vector3 n_rotation, float duration, RotateMode mode, Ease ease, bool use,
            AnimationCurve curve)
        {
            if (RotationTween != null)
            {
                RotationTween.Kill();
                RotationTween = null;
            }

            RotationTween = !use
                ? rotator.DOLocalRotate(n_rotation, duration, mode).SetEase(ease)
                : rotator.DOLocalRotate(n_rotation, duration, mode).SetEase(curve);
        }

        private void SetScale(Vector3 n_scale, float duration, Ease ease, bool use, AnimationCurve curve)
        {
            if (ScaleTween != null)
            {
                ScaleTween.Kill();
                ScaleTween = null;
            }

            ScaleTween = !use
                ? scale.DOScale(n_scale, duration).SetEase(ease)
                : scale.DOScale(n_scale, duration).SetEase(curve);
        }

        private void SetFov(float n_fov, float duration, Ease ease, bool use, AnimationCurve curve)
        {
            if (FovTween != null)
            {
                FovTween.Kill();
                FovTween = null;
            }

            FovTween = !use
                ? FollowingCamera.DOFieldOfView(n_fov, duration).SetEase(ease)
                : FollowingCamera.DOFieldOfView(n_fov, duration).SetEase(curve);
        }

        public void DoShake(float power = 1f, float duration = 3f)
        {
            if (ShakeTween != null)
            {
                ShakeTween.Kill();
                ShakeTween = null;
            }

            ShakeTween = DOTween.To(() => ShakePower, x => ShakePower = x, power, duration * 0.5f).SetEase(Ease.Linear);
            ShakeTween.SetLoops(2, LoopType.Yoyo);
            ShakeTween.OnUpdate(ShakeUpdate);
            ShakeTween.OnComplete(ShakeFinished);
        }

        private void ShakeUpdate()
        {
            scale.transform.localPosition = new Vector3(UnityEngine.Random.value * ShakePower,
                UnityEngine.Random.value * ShakePower, UnityEngine.Random.value * ShakePower);
        }

        private void ShakeFinished()
        {
            scale.transform.localPosition = Vector3.zero;
        }

        private void SetDefaultTransform()
        {
            rotator.localPosition = defaultOffset;
            rotator.eulerAngles = defaultRotation - new Vector3(60f, 0f, 0f);
            scale.localScale = defaultScale;
        }

        private static Vector3 GetRotatingVector(Vector3 first, Vector3 second, bool positive)
        {
            return positive
                ? 0.5f * (first.Convert() + second.Convert())
                : -0.5f * (first.Convert() + second.Convert());
        }

        public void SetRotatingOrigin(Vector3 first, Vector3 second)
        {
            var eulerRotation = Quaternion.Euler(-0.5f * (first.Convert() + second.Convert()));
            var eulerOrigin = Quaternion.Euler(0.5f * (first.Convert() + second.Convert()));
            Rotation = eulerRotation;
            Origin.rotation = eulerOrigin;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!Application.isPlaying)
                SetDefaultTransform();
        }
#endif
    }

    [Serializable]
    public class CameraSettings
    {
        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale;
        public float fov;
        public bool follow;

        public CameraSettings GetCamera()
        {
            var settings = new CameraSettings();
            var follower = CameraFollower.Instance;
            settings.offset = follower.rotator.localPosition;
            settings.rotation = follower.rotator.localEulerAngles + new Vector3(60f, 0f, 0f);
            settings.scale = follower.scale.localScale;
            settings.fov = follower.FollowingCamera.fieldOfView;
            settings.follow = follower.follow;
            return settings;
        }

        public void SetCamera(Vector3 first, Vector3 second)
        {
            var follower = CameraFollower.Instance;
            follower.rotator.localPosition = offset;
            follower.rotator.localEulerAngles = rotation - new Vector3(60f, 0f, 0f);
            follower.scale.localScale = scale;
            follower.scale.localPosition = Vector3.zero;
            follower.FollowingCamera.fieldOfView = fov;
            follower.follow = follow;

            CameraFollower.Instance.SetRotatingOrigin(first, second);
        }
    }
}