using DancingLineFanmade.Level;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DancingLineFanmade.UI
{
    [DisallowMultipleComponent]
    public class LoadingPage : MonoBehaviour
    {
        public static LoadingPage Instance { get; private set; }

        [SerializeField] private Text loadingText;
        [SerializeField] private Image background;
        [SerializeField] private Image loadingImage;

        private CanvasGroup canvasGroup;
        private AsyncOperation operation = null;

        private void Awake()
        {
            Instance = this;
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Start()
        {
            canvasGroup.alpha = 0f;
        }

        public Tween Fade(float alpha, float duration, Ease ease = Ease.Linear)
        {
            return canvasGroup.DOFade(alpha, duration).SetEase(ease);
        }

        public void Load(string sceneName)
        {
            Color backgroundColor = Player.Instance.sceneCamera.backgroundColor;

            background.color = backgroundColor;
            loadingText.color = LevelManager.GetColorByContent(backgroundColor);
            loadingImage.color = LevelManager.GetColorByContent(backgroundColor);

            Fade(1f, 0.4f).OnComplete(() =>
            {
                operation = SceneManager.LoadSceneAsync(sceneName);
                if (operation.isDone) operation = null;
            });
        }
    }
}