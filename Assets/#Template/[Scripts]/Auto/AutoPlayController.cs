using DancingLineFanmade.Guidance;
using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Auto
{
    [DisallowMultipleComponent]
    public class AutoPlayController : MonoBehaviour
    {
        public static AutoPlayController Instance { get; private set; }

        private GuidanceController controller;
        internal Transform holder;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            controller = GuidanceController.Instance ? GuidanceController.Instance : null;
            GuidanceBox[] boxes = controller ? controller.boxHolder ? controller.boxHolder.GetComponentsInChildren<GuidanceBox>() : null : null;
            holder = (controller && boxes != null) ? new GameObject("AutoPlayHolder").transform : null;

            if (controller && boxes != null)
            {
                for (int a = 1; a < boxes.Length; a++)
                {
                    GameObject obj = LevelManager.CreateTrigger(boxes[a].transform.position, Vector3.zero, Vector3.one * 4, false, "AutoPlayTrigger " + a);
                    obj.AddComponent<AutoPlay>();
                    obj.transform.parent = holder;
                }
            }
            SetHolder(false);
        }

        public void SetHolder(bool active)
        {
            holder?.gameObject.SetActive(active);
        }
    }
}