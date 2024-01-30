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
        public Camera thisCamera { get; set; }

        [SerializeField] private Transform target;

        [SerializeField] internal Vector3 followSpeed = new Vector3(1.5f, 1.5f, 1.5f);
        [SerializeField] internal bool follow = true;
        [SerializeField] internal bool smooth = true;

        internal Transform rotator;
        internal Transform scale;

        private Tween offset;
        private Tween rotation;
        private Tween zoom;
        private Tween shake;
        private Tween fov;

        private float shakePower { get; set; }

        private void Awake()
        {
            Instance = this;
            selfTransform = transform;
        }

        private void Start()
        {
            rotator = selfTransform.Find("Rotator");
            scale = rotator.Find("Scale");
            thisCamera = scale.Find("Camera").GetComponent<Camera>();
        }

        private void Update()
        {
            Vector3 translation = target.position - selfTransform.position;
            if (LevelManager.GameState == GameStatus.Playing && follow)
            {
                if (smooth) selfTransform.Translate(new Vector3(translation.x * followSpeed.x * Time.deltaTime, translation.y * followSpeed.y * Time.deltaTime, translation.z * followSpeed.z * Time.deltaTime));
                else selfTransform.position = target.position;
            }
        }

        internal void Trigger(Vector3 offset, Vector3 rotation, Vector3 scale, float fov, float duration, Ease ease, RotateMode mode, UnityEvent callback)
        {
            SetOffset(offset, duration, ease);
            SetRotation(rotation, duration, mode, ease);
            SetScale(scale, duration, ease);
            SetFov(fov, duration, ease);
            this.rotation.OnComplete(() => callback.Invoke());
        }

        internal void KillAll()
        {
            offset?.Kill();
            rotation?.Kill();
            zoom?.Kill();
            shake?.Kill();
            fov?.Kill();
        }

        private void SetOffset(Vector3 offset, float duration, Ease ease = Ease.InOutSine)
        {
            if (this.offset != null)
            {
                this.offset.Kill();
                this.offset = null;
            }
            this.offset = rotator.DOLocalMove(offset, duration).SetEase(ease);
        }

        private void SetRotation(Vector3 rotation, float duration, RotateMode mode, Ease ease = Ease.InOutSine)
        {
            if (this.rotation != null)
            {
                this.rotation.Kill();
                this.rotation = null;
            }
            this.rotation = rotator.DOLocalRotate(rotation, duration, mode).SetEase(ease);
        }

        private void SetScale(Vector3 scale, float duration, Ease ease = Ease.InOutSine)
        {
            if (zoom != null)
            {
                zoom.Kill();
                zoom = null;
            }
            zoom = this.scale.DOScale(scale, duration).SetEase(ease);
        }

        public void DoShake(float power = 1f, float duration = 3f)
        {
            if (shake != null)
            {
                shake.Kill();
                shake = null;
            }
            shake = DOTween.To(() => shakePower, x => shakePower = x, power, duration * 0.5f).SetEase(Ease.Linear);
            shake.SetLoops(2, LoopType.Yoyo);
            shake.OnUpdate(new TweenCallback(ShakeUpdate));
            shake.OnComplete(new TweenCallback(ShakeFinished));
        }

        private void ShakeUpdate()
        {
            scale.transform.localPosition = new Vector3(UnityEngine.Random.value * shakePower, UnityEngine.Random.value * shakePower, UnityEngine.Random.value * shakePower);
        }

        private void ShakeFinished()
        {
            scale.transform.localPosition = Vector3.zero;
        }

        private void SetFov(float fov, float duration, Ease ease = Ease.InOutSine)
        {
            if (this.fov != null)
            {
                this.fov.Kill();
                this.fov = null;
            }
            this.fov = thisCamera.DOFieldOfView(fov, duration).SetEase(ease);
        }
    }

    [Serializable]
    public class CameraSettings
    {
        public Vector3 offset;
        public Vector3 rotation;
        public Vector3 scale;
        public float fov;
        public bool follow;

        internal CameraSettings GetCamera()
        {
            CameraSettings settings = new CameraSettings();
            CameraFollower follower = CameraFollower.Instance;
            settings.offset = follower.rotator.localPosition;
            settings.rotation = follower.rotator.localEulerAngles;
            settings.scale = follower.scale.localScale;
            settings.fov = follower.thisCamera.fieldOfView;
            settings.follow = follower.follow;
            return settings;
        }

        internal void SetCamera()
        {
            CameraFollower follower = CameraFollower.Instance;
            follower.rotator.localPosition = offset;
            follower.rotator.localEulerAngles = rotation;
            follower.scale.localScale = scale;
            follower.scale.localPosition = Vector3.zero;
            follower.thisCamera.fieldOfView = fov;
            follower.follow = follow;
        }
    }
}