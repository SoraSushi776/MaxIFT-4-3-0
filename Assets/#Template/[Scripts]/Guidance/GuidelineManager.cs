using Sirenix.OdinInspector;
using System.Collections.Generic;
using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Guideline
{
    [DisallowMultipleComponent]
    public class GuidelineManager : MonoBehaviour
    {
        [Title("Guideline Generator Setting"), SerializeField]
        internal Transform guidelineTapHolder;

        [SerializeField] internal List<Color> colors = new() 
        { Color.white, Color.black };
        [SerializeField, Min(0f)] internal float lineGap = 0.2f;
        [SerializeField] internal bool autoplay;
        [SerializeField] internal Vector3 autoplayTriggerSize = new(0.5f, 0.5f, 3f);

        [Title("Road Generator Setting"), SerializeField]
        private GameObject roadPrefab;

        [SerializeField] private float width = 2f;
        [SerializeField] private Vector3 offset;

        private GameObject linePrefab;
        private readonly List<GuidelineTap> boxes = new();

        private void Start()
        {
            if (!guidelineTapHolder)
                return;
            boxes.AddRange(guidelineTapHolder.GetComponentsInChildren<GuidelineTap>());
            linePrefab = Resources.Load<GameObject>("Prefabs/Guideline");
            for (var i = 0; i < boxes.Count; i++)
            {
                if (i < boxes.Count - 1 && boxes[i].haveLine)
                    GenerateLine(boxes[i].transform, boxes[i + 1].transform);
                boxes[i].InitBox(autoplay);
                boxes[i].SetColor(colors);
                if (autoplay)
                    boxes[i].AddBoxCollider(autoplayTriggerSize);
            }

            SetUseGuideline();
            if (autoplay)
                Player.Instance.disallowInput = true;

            LevelManager.revivePlayer += ResetTapInternal;
        }

        private void OnDestroy()
        {
            LevelManager.revivePlayer -= ResetTapInternal;
        }

        private void GenerateLine(Transform box1, Transform box2)
        {
            var difference = box2.position - box1.position;
            var length = difference.magnitude - lineGap * 2 - 1.5f;

            if (!(length > 0))
                return;
            var middlePosition = (box1.position + box2.position) * 0.5f;
            var targetRotation = Quaternion.LookRotation(difference) * Quaternion.Euler(90, 0, 90);
            var line = Instantiate(linePrefab, middlePosition, Quaternion.Euler(-90, 0, 0)).transform;
            var sprite = line.GetComponent<SpriteRenderer>();
            var box = box1.GetComponent<GuidelineTap>();

            line.localScale = new Vector3(length, 0.15f, 1f);
            line.rotation = targetRotation;
            line.SetParent(box1);
            sprite.color = colors[box.colorIndex];
        }

        public void SetUseGuideline(bool useGuideline = true)
        {
            if (!guidelineTapHolder)
                return;
            guidelineTapHolder.position = useGuideline ? Vector3.zero : Vector3.one * -99999f;
            guidelineTapHolder.gameObject.SetActive(useGuideline);
        }

        private void ResetTapInternal()
        {
            ResetAllTaps(Player.Instance.SoundTrack.time);
        }

        public void ResetAllTaps(float time)
        {
            foreach (var VARIABLE in boxes)
            {
                VARIABLE.InitBox(false);
                VARIABLE.triggered = false;
                if (!(time > VARIABLE.displayTime))
                    continue;
                VARIABLE.SetDisplay(true);
                if (!(VARIABLE.triggerTime < time))
                    continue;
                VARIABLE.SetDisplay(false);
                VARIABLE.triggered = true;
            }
        }

        [Button("Create Road By Guideline Taps", ButtonSizes.Large)]
        private void CreateRoad()
        {
#if UNITY_EDITOR
            if (!guidelineTapHolder)
                Debug.LogError("引导线父物体未选择。");
            else
            {
                var taps = guidelineTapHolder.GetComponentsInChildren<GuidelineTap>();
                var holder = new GameObject("RoadHolder").transform;
                for (var i = 0; i < taps.Length; i++)
                {
                    if (i + 1 >= taps.Length)
                        continue;
                    var difference = taps[i + 1].transform.position - taps[i].transform.position;
                    var length = difference.magnitude;
                    var middlePosition = (taps[i].transform.position + taps[i + 1].transform.position) * 0.5f;
                    var targetRotation = Quaternion.LookRotation(difference) * Quaternion.Euler(0, 90, 0);
                    var road = Instantiate(roadPrefab, middlePosition - offset, Quaternion.Euler(Vector3.zero))
                        .transform;

                    road.localScale = new Vector3(length + width, 1f, width);
                    road.rotation = targetRotation;
                    road.transform.SetParent(holder);
                }
            }
#endif
        }
    }
}