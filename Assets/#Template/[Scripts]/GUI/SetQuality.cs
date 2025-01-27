using DancingLineFanmade.Level;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

namespace DancingLineFanmade.UI
{
    [DisallowMultipleComponent]
    public class SetQuality : MonoBehaviour
    {
        [SerializeField] private Text text;
        PostProcessVolume post;

        private int id = 0;

        private void Start()
        {
            id = QualitySettings.GetQualityLevel();
            SetText();
            foreach (ActiveByQuality a in FindObjectsOfType<ActiveByQuality>(true)) a.OnEnable();
        }

        public void SetLevel(bool add)
        {
            if (add) id = id++ >= 4 ? id = 0 : id++;
            else id = id-- <= 0 ? id = 4 : id--;
            QualitySettings.SetQualityLevel(id);
            SetText();
            foreach (ActiveByQuality a in FindObjectsOfType<ActiveByQuality>(true)) a.OnEnable();
        }

        private void SetText()
        {
            post = FindObjectOfType<PostProcessVolume>();
            LevelManager.SetFPSLimit(int.MaxValue);
#if UNITY_ANDROID
            QualitySettings.shadows = ShadowQuality.Disable;
#endif
            switch (id)
            {
                case 0:
                    text.text = "低";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.Disable;
                    post.enabled = false;
#endif
                    break;
                case 1:
                    text.text = "中";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.Disable;
                    post.enabled = false;
#endif
                    break;
                case 2:
                    text.text = "高";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.All;
                    post.enabled = false;
#endif
                    break;
                case 3:
                    text.text = "精";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.All;
                    post.enabled = true;
#endif
                    break;
                case 4:
                    text.text = "极";
#if UNITY_STANDALONE || UNITY_IOS || UNITY_EDITOR
                    QualitySettings.shadows = ShadowQuality.All;
                    post.enabled = true;
#endif
                    break;
            }
        }
    }
}