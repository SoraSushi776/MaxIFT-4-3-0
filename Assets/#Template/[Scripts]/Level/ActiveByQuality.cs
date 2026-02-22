using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Level
{
    public enum ActiveType
    {
        [LabelText("≥ 此等级时 显示")]
        ShowWhenEqualOrHigher,

        [LabelText("< 此等级时 显示")]
        ShowWhenLower,

        [LabelText("≥ 此等级时 隐藏")]
        HideWhenEqualOrHigher,

        [LabelText("< 此等级时 隐藏")]
        HideWhenLower,
    }

    [DisallowMultipleComponent]
    public class ActiveByQuality : MonoBehaviour
    {
        [Title("画质控制")]
        [SerializeField, EnumToggleButtons, DisableInPlayMode]
        private ActiveType activeType = ActiveType.ShowWhenEqualOrHigher;

        [SerializeField, DisableInPlayMode, PropertyRange(min: 0, maxGetter: "@HighestQualityIndex")]
        [InfoBox("$qualityTip", InfoMessageType.Info)]
        private int targetQualityIndex = 2;
        private int HighestQualityIndex => QualitySettings.names != null ? QualitySettings.names.Length - 1 : -1;

        [SerializeField, HideInInspector]
        private string qualityTip = "正在计算画质提示...";

        [Space(10)]
        [SerializeField, PropertyOrder(100)]
        private UnityEvent<bool> onQualityApplied = new UnityEvent<bool>();

        private string CalculateQualityTip()
        {
            string[] qualityNames = QualitySettings.names;
            if (qualityNames == null || qualityNames.Length == 0)
                return "暂无法读取画质设置";

            string relation = activeType switch
            {
                ActiveType.ShowWhenEqualOrHigher  => "≥",
                ActiveType.ShowWhenLower          => "<",
                ActiveType.HideWhenEqualOrHigher  => "≥",
                ActiveType.HideWhenLower          => "<",
                _ => "？"
            };

            string action = activeType.ToString().StartsWith("Show") ? "显示" : "隐藏";
            string qualityName = qualityNames[targetQualityIndex];

            return $"当前画质 {relation} {qualityName}（{targetQualityIndex}）时 {action}此物体";
        }

        public void OnEnable()
        {
            ApplyByCurrentQuality();
        }

        private void ApplyByCurrentQuality()
        {
            if (!this || !gameObject) return;

            int currentLevel = QualitySettings.GetQualityLevel();
            bool shouldActive = false;

            switch (activeType)
            {
                case ActiveType.ShowWhenEqualOrHigher:
                    shouldActive = currentLevel >= targetQualityIndex;
                    break;
                case ActiveType.ShowWhenLower:
                    shouldActive = currentLevel < targetQualityIndex;
                    break;
                case ActiveType.HideWhenEqualOrHigher:
                    shouldActive = currentLevel < targetQualityIndex;
                    break;
                case ActiveType.HideWhenLower:
                    shouldActive = currentLevel >= targetQualityIndex;
                    break;
            }

            if (gameObject.activeSelf != shouldActive)
            {
                gameObject.SetActive(shouldActive);
            }

            onQualityApplied?.Invoke(shouldActive);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            qualityTip = CalculateQualityTip();
        }
#endif
    }
}