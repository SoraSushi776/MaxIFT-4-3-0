using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DancingLineFanmade.UI
{
    [DisallowMultipleComponent]
    public class StartPage : MonoBehaviour
    {
        [SerializeField] private List<RectTransform> moveLeft;
        [SerializeField] private List<RectTransform> moveDown;
        [SerializeField] private List<RectTransform> moveUp;

        private void OnEnable()
        {
#if UNITY_EDITOR
            foreach (RectTransform g in moveUp) g.gameObject.SetActive(true);
#else
            foreach(RectTransform g in moveUp) g.gameObject.SetActive(false);
#endif
        }

        public void Hide()
        {
            foreach (RectTransform l in moveLeft)
            {
                if (l.GetComponent<Button>()) l.GetComponent<Button>().interactable = false;
                l.DOAnchorPos(new Vector2(-120f, l.anchoredPosition.y), 0.4f).SetEase(Ease.InSine).OnComplete(() => { Destroy(gameObject); });
            }
            foreach (RectTransform d in moveDown)
            {
                if (d.GetComponent<Button>()) d.GetComponent<Button>().interactable = false;
                d.DOAnchorPos(new Vector2(d.anchoredPosition.x, -250f), 0.4f).SetEase(Ease.InSine);
            }
#if UNITY_EDITOR
            foreach (RectTransform u in moveUp)
            {
                if (u.GetComponent<Toggle>()) u.GetComponent<Toggle>().interactable = false;
                u.DOAnchorPos(new Vector2(u.anchoredPosition.x, 100f), 0.4f).SetEase(Ease.InSine);
            }
#endif
        }
    }
}