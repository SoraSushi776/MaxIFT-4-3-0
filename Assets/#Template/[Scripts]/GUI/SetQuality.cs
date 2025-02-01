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
            QualitySettings.shadows = ShadowQuality.Disable;
            switch (id)
            {
                case 0:
                    text.text = "��";
                    QualitySettings.shadows = ShadowQuality.Disable;
                    if(post != null){post.enabled = false;}
                    break;
                case 1:
                    text.text = "��";
                    QualitySettings.shadows = ShadowQuality.Disable;
                    if(post != null){post.enabled = false;}
                    break;
                case 2:
                    text.text = "��";
                    QualitySettings.shadows = ShadowQuality.All;
                    if(post != null){post.enabled = false;}
                    break;
                case 3:
                    text.text = "����";
                    QualitySettings.shadows = ShadowQuality.All;
                    if(post != null){post.enabled = true;}
                    break;
                case 4:
                    text.text = "����";
                    QualitySettings.shadows = ShadowQuality.All;
                    if(post != null){post.enabled = true;}
                    break;
            }
        }
    }
}