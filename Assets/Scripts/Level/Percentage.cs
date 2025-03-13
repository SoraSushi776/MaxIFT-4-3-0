using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace DancingLineFanmade.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public class Percentage : MonoBehaviour
    {
        [SerializeField, EnumToggleButtons] private Percent percent = Percent.Ten;
        [SerializeField] private Color color = Color.black;
        [SerializeField] private PercentageIcons icons = new PercentageIcons();

        private SpriteRenderer spriteRenderer;
        [SerializeField, HideInInspector] private Material material;

        private void Start()
        {
            SetPercentage();
        }

        private void OnValidate()
        {
            SetPercentage();
        }

        private void SetPercentage()
        {
            spriteRenderer = GetComponent<SpriteRenderer>() ? GetComponent<SpriteRenderer>() : gameObject.AddComponent<SpriteRenderer>();
            spriteRenderer.material = material;
            spriteRenderer.color = color;
            switch (percent)
            {
                case Percent.Ten:
                    spriteRenderer.sprite = icons.ten;
                    gameObject.name = "10%";
                    break;
                case Percent.Twenty:
                    spriteRenderer.sprite = icons.twenty;
                    gameObject.name = "20%";
                    break;
                case Percent.Thirty:
                    spriteRenderer.sprite = icons.thirty;
                    gameObject.name = "30%";
                    break;
                case Percent.Forty:
                    spriteRenderer.sprite = icons.forty;
                    gameObject.name = "40%";
                    break;
                case Percent.Fifty:
                    spriteRenderer.sprite = icons.fifty;
                    gameObject.name = "50%";
                    break;
                case Percent.Sixty:
                    spriteRenderer.sprite = icons.sixty;
                    gameObject.name = "60%";
                    break;
                case Percent.Seventy:
                    spriteRenderer.sprite = icons.seventy;
                    gameObject.name = "70%";
                    break;
                case Percent.Eighty:
                    spriteRenderer.sprite = icons.eighty;
                    gameObject.name = "80%";
                    break;
                case Percent.Ninety:
                    spriteRenderer.sprite = icons.ninety;
                    gameObject.name = "90%";
                    break;
            }
        }
    }

    [Serializable]
    public struct PercentageIcons
    {
        public Sprite ten;
        public Sprite twenty;
        public Sprite thirty;
        public Sprite forty;
        public Sprite fifty;
        public Sprite sixty;
        public Sprite seventy;
        public Sprite eighty;
        public Sprite ninety;
    }
}