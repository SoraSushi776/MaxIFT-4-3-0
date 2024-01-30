using DancingLineFanmade.Level;
using UnityEngine;
using UnityEngine.UI;

namespace DancingLineFanmade.UI
{
    [DisallowMultipleComponent]
    public class SetQuality : MonoBehaviour
    {
        [SerializeField] private Text text;

        private int id = 0;

        private void Start()
        {
            id = QualitySettings.GetQualityLevel();
            SetText();
            foreach (ActiveByQuality a in FindObjectsOfType<ActiveByQuality>(true)) a.OnEnable();
        }

        public void SetLevel(bool add)
        {
            if (add) id = id++ >= 2 ? id = 0 : id++;
            else id = id-- <= 0 ? id = 2 : id--;
            QualitySettings.SetQualityLevel(id);
            SetText();
            foreach (ActiveByQuality a in FindObjectsOfType<ActiveByQuality>(true)) a.OnEnable();
        }

        private void SetText()
        {
            LevelManager.SetFPSLimit(int.MaxValue);
#if UNITY_ANDROID
            QualitySettings.shadows = ShadowQuality.Disable;
#endif
            switch (id)
            {
                case 0:
                    text.text = "µÍ";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.Disable;
#endif
                    break;
                case 1:
                    text.text = "ÖÐ";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.Disable;
#endif
                    break;
                case 2:
                    text.text = "¸ß";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.All;
#endif
                    break;
            }
        }
    }
}