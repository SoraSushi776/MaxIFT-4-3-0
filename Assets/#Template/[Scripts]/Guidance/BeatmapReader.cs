using System.Collections.Generic;
using System.Linq;
using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Guideline
{
    [DisallowMultipleComponent]
    public class BeatmapReader : MonoBehaviour
    {
        [SerializeField] private Player player;
        [SerializeField] private TextAsset beatmap;
        [SerializeField] private float offset;

        [SerializeField] internal List<float> hitTime;

        private readonly List<string> hit1 = new();
        private readonly List<List<string>> hit2 = new();

#if UNITY_EDITOR
        private void ReadBeatmap()
        {
            hit1.Clear();
            hit2.Clear();
            hitTime.Clear();

            if (beatmap == null)
            {
                Debug.LogError("未选择谱面数据文件。");
                return;
            }

            foreach (var VARIABLE in beatmap.text.Split('\n'))
            {
                hit1.Add(VARIABLE.Trim());
            }

            var index = hit1.IndexOf("[HitObjects]");
            hit1.RemoveRange(0, index + 1);
            hit1.RemoveAll(text => text == string.Empty);

            foreach (var VARIABLE in hit1)
            {
                hit2.Add(VARIABLE.Split(',').ToList());
            }

            foreach (var VARIABLE in hit2)
            {
                hitTime.Add(int.Parse(VARIABLE[2]) / 1000f + offset);
            }
        }

        [Button("Create Guideline Taps By Beatmap", ButtonSizes.Large)]
        private void CreateGuidelineTaps()
        {
            ReadBeatmap();

            if (hitTime.Count <= 0)
                return;
            var boxPrefab = Resources.Load<GameObject>("Prefabs/GuidelineTap");
            var startPos = player.startPosition;
            var firstDir = player.firstDirection;
            var secondDir = player.secondDirection;
            var speed = player.levelData.speed;
            var hitParent = new GameObject("GuidelineTapHolder-BeatmapCreated");
            var count = 1;

            var boxes = new List<GameObject>();
            var firstBox = Instantiate(boxPrefab, startPos - new Vector3(0f, 0.4f, 0f),
                Quaternion.Euler(90, firstDir.y, 0));
            firstBox.GetComponent<GuidelineTap>().triggered = true;
            firstBox.transform.parent = hitParent.transform;
            boxes.Add(firstBox);

            for (var i = 0; i < hitTime.Count; i++)
            {
                var focusedBox = Instantiate(boxPrefab, hitParent.transform, true);
                var box = focusedBox.GetComponent<GuidelineTap>();
                if (boxes.Count > 0)
                    focusedBox.transform.position = boxes[^1].transform.position;
                else focusedBox.transform.position = startPos - new Vector3(0f, 0.4f, 0f);

                focusedBox.transform.eulerAngles = (count % 2) switch
                {
                    1 => new Vector3(90, firstDir.y, 0),
                    0 => new Vector3(90, secondDir.y, 0),
                    _ => focusedBox.transform.eulerAngles
                };

                focusedBox.transform.Translate(
                    i == 0
                        ? new Vector3(0, hitTime[i] * speed, 0)
                        : new Vector3(0, (hitTime[i] - hitTime[i - 1]) * speed, 0), Space.Self);
                box.triggerTime = hitTime[i];
                box.displayTime = hitTime[i] - 2.5f < 0f ? 0f : hitTime[i] - 2.5f;
                boxes.Add(focusedBox);
                count++;
            }

            for (var i = 0; i < boxes.Count; i++)
            {
                boxes[i].transform.eulerAngles = ((i + 1) % 2) switch
                {
                    1 => new Vector3(90, firstDir.y, 0),
                    0 => new Vector3(90, secondDir.y, 0),
                    _ => boxes[i].transform.eulerAngles
                };
            }
        }

        [Button("Reload Hit Time", ButtonSizes.Large)]
        private void ReloadHits()
        {
            ReadBeatmap();
        }
#endif
    }
}