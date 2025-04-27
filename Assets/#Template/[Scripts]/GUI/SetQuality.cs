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

            int maxFrame = 120;
        #if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject windowManager = currentActivity.Call<AndroidJavaObject>("getWindowManager"))
                using (AndroidJavaObject display = windowManager.Call<AndroidJavaObject>("getDefaultDisplay"))
                {
                    maxFrame = (int)display.Call<float>("getRefreshRate");
                }
            }
            catch
            {
                maxFrame = 120;
            }
        #elif UNITY_IOS && !UNITY_EDITOR
            maxFrame = UnityEngine.iOS.Device.generation.ToString().Contains("ProMotion") ? 120 : 60;
        #else
            maxFrame = Screen.currentResolution.refreshRate;
        #endif
            Application.targetFrameRate = maxFrame;
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
            QualitySettings.shadows = ShadowQuality.Disable;
            switch (id)
            {
                case 0:
                    text.text = "低";
                    QualitySettings.shadows = ShadowQuality.Disable;
                    if(post != null){post.enabled = false;}
                    break;
                case 1:
                    text.text = "中";
                    QualitySettings.shadows = ShadowQuality.Disable;
                    if(post != null){post.enabled = false;}
                    break;
                case 2:
                    text.text = "高";
                    QualitySettings.shadows = ShadowQuality.All;
                    if(post != null){post.enabled = false;}
                    break;
                case 3:
                    text.text = "极高";
                    QualitySettings.shadows = ShadowQuality.All;
                    if(post != null){post.enabled = true;}
                    break;
                case 4:
                    text.text = "极致";
                    QualitySettings.shadows = ShadowQuality.All;
                    if(post != null){post.enabled = true;}
                    break;
            }
        }
    }
}