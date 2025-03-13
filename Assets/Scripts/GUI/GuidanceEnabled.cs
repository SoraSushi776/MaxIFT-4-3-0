using UnityEngine;
using UnityEngine.UI;

namespace DancingLineFanmade.Guidance
{
    [DisallowMultipleComponent]
    public class GuidanceEnabled : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Image background;
        [SerializeField] private Sprite on;
        [SerializeField] private Sprite off;
        [SerializeField] private new bool enabled = false;

        private GuidanceController controller;

        private void Start()
        {
            controller = FindObjectOfType<GuidanceController>();
            SetGuidance(enabled);

            if (!controller.boxHolder)
            {
                GetComponent<Button>().interactable = false;
                foreach (Image i in GetComponentsInChildren<Image>())
                {
                    i.enabled = false;
                    i.raycastTarget = false;
                }
                background.enabled = false;
                background.raycastTarget = false;
            }
        }

        public void OnClick()
        {
            enabled = !enabled;
            SetGuidance(enabled);
        }

        private void SetGuidance(bool enabled)
        {
            if (enabled)
            {
                image.sprite = on;
                if (controller.boxHolder) controller.boxHolder.gameObject.SetActive(true);
            }
            else
            {
                image.sprite = off;
                if (controller.boxHolder) controller.boxHolder.gameObject.SetActive(false);
            }
        }
    }
}