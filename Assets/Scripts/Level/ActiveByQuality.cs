using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Level
{
    public enum ActiveType
    {
        Display,
        Hide
    }

    public enum QualityLevel
    {
        Low,
        Medium,
        High
    }

    [DisallowMultipleComponent]
    public class ActiveByQuality : MonoBehaviour
    {
        [SerializeField, EnumToggleButtons, InfoBox("$message"), DisableInPlayMode] private ActiveType activeType = ActiveType.Hide;
        [SerializeField, EnumToggleButtons, DisableInPlayMode] private QualityLevel targetLevel = QualityLevel.Medium;

        private string message;

        internal void OnEnable()
        {
            int i;

            switch (targetLevel)
            {
                case QualityLevel.Low: i = 0; break;
                case QualityLevel.Medium: i = 1; break;
                case QualityLevel.High: i = 2; break;
                default: i = -1; break;
            }
            if (activeType == ActiveType.Display) if (QualitySettings.GetQualityLevel() > i) gameObject.SetActive(true); else gameObject.SetActive(false);
            if (activeType == ActiveType.Hide) if (QualitySettings.GetQualityLevel() < i) gameObject.SetActive(false); else gameObject.SetActive(true);
        }

        private void OnValidate()
        {
            string text1;
            string text2;
            string text3;

            if (activeType == ActiveType.Display)
            {
                text1 = "显示";
                text2 = "高于";
            }
            else
            {
                text1 = "隐藏";
                text2 = "低于";
            }
            switch (targetLevel)
            {
                case QualityLevel.Low: text3 = "低画质"; break;
                case QualityLevel.Medium: text3 = "中画质"; break;
                case QualityLevel.High: text3 = "高画质"; break;
                default: text3 = "-"; break;
            }

            message = "当画质" + text2 + text3 + "时" + text1;
        }
    }
}