using System.Collections;
using DancingLineFanmade.Level;
using DG.Tweening;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Trigger
{
    public class CameraTrigger : MonoBehaviour
    {
        private CameraFollower follower;
        [SerializeField] private UnityEvent onFinished = new UnityEvent();
        [SerializeField] public Vector3 offset = Vector3.zero;
        [SerializeField] public Vector3 rotation = new Vector3(90f, 45f, 0f);
        [SerializeField] public Vector3 scale = Vector3.one;
        [SerializeField, Range(0f, 179f)] public float fieldOfView = 60f;
        [SerializeField] private bool follow = true;
        [SerializeField, MinValue(0f)] private float duration = 2f;
        [SerializeField] private Ease ease = Ease.InOutSine;
        [SerializeField] private RotateMode mode = RotateMode.Fast;
        [Space]
        [SerializeField] private bool canBeTriggered = true;

        [SerializeField, HideInInspector] private PreviewCamera previewCamera;

        private void Start()
        {
            follower = CameraFollower.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && canBeTriggered)
            {
                follower.follow = follow;
                follower.Trigger(offset, rotation, scale, fieldOfView, duration, ease, mode, onFinished, false, null);
            }
        }

        public void Trigger()
        {
            if (!canBeTriggered)
            {
                follower.follow = follow;
                follower.Trigger(offset, rotation, scale, fieldOfView, duration, ease, mode, onFinished, false, null);
            }
        }

#if UNITY_EDITOR

        [Button("Add Preview Camera", ButtonSizes.Large), HideIf("@previewCamera != null")]
        private void Add()
        {
            var obj = new GameObject("PreviewCamera");
            obj.transform.parent = this.transform;
            obj.transform.localPosition = Vector3.zero;
            previewCamera = obj.AddComponent<PreviewCamera>();
            obj.name = $"PreviewCamera of {this.name}";

            previewCamera.rotator = new GameObject("Rotator").transform;
            previewCamera.rotator.parent = obj.transform;
            previewCamera.rotator.localPosition = Vector3.zero;

            previewCamera.scale = new GameObject("Scale").transform;
            previewCamera.scale.parent = previewCamera.rotator;
            previewCamera.scale.localPosition = Vector3.zero;

            Transform cam_temp = new GameObject("Camera").transform;
            cam_temp.parent = previewCamera.scale;
            cam_temp.localPosition = new (0, 0, -15f);
            previewCamera.cam = cam_temp.gameObject.AddComponent<Camera>();

            previewCamera.cam.tag = "EditorOnly";
            StartCoroutine(UpdatePreview());
        }

        IEnumerator UpdatePreview()
        {
            yield return null;
            previewCamera?.UpdateCam(this);
        }

        [Button("Delete Preview Camera", ButtonSizes.Large), HideIf("@previewCamera == null")]
        private void Delete()
        {
            DestroyImmediate(previewCamera.gameObject);
        }

        private void OnValidate()
        {
            if (previewCamera != null)
            {
                previewCamera.UpdateCam(this);
            }
        }

#endif
    }
}