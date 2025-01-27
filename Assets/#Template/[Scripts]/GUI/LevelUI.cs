using DancingLineFanmade.Level;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using DancingLineFanmade.Trigger;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using JetBrains.Annotations;

namespace DancingLineFanmade.UI
{
    public class LevelUI : MonoBehaviour
    {
        public readonly Color fade = new(1,1,1,0);
        public static LevelUI Instance { get; private set; }
        ParticleSystem system;

        [Title("Normal")]
        [SerializeField] private Text title;
        [SerializeField] private Text percentage;
        [SerializeField] private Text block;
        [SerializeField] private Image background;
        [SerializeField] private RectTransform barFill;
        [SerializeField] private RectTransform moveUpPart;
        [SerializeField] private RectTransform moveDownPart;
        [SerializeField] private List<CanvasGroup> normalAlpha = new List<CanvasGroup>();
        [SerializeField] private List<Image> crownInfill = new List<Image>();
        [SerializeField] private List<Image> crownDisperse = new List<Image>();
        [SerializeField] public Vector2 m_Scale;
        [SerializeField] private List<AudioClip> crownSount = new List<AudioClip>();
        public List<RawImage> crownParticlesImage = new();
        [SerializeField] private List<Button> buttons = new List<Button>();

        [Title("Revive")]
        [SerializeField] private Text percentageRevive;
        [SerializeField] private RectTransform barFillRevive;
        [SerializeField] private RectTransform moveUpRevive;
        [SerializeField] private RectTransform moveDownRevive;
        [SerializeField] private Image hideScreenImage;
        [SerializeField] private List<CanvasGroup> reviveAlpha = new List<CanvasGroup>();
        [SerializeField] private List<Button> buttonsRevive = new List<Button>();

        private Player player;
        private float progress;

        private void Awake()
        {
            Instance = this;
            player = Player.Instance;

            moveUpPart.anchoredPosition = new Vector2(0f, -250f);
            moveDownPart.anchoredPosition = new Vector2(0f, 430f);
            moveUpRevive.anchoredPosition = new Vector2(0f, -250f);
            moveDownRevive.anchoredPosition = new Vector2(0f, 260f);

            foreach (CanvasGroup group in normalAlpha) group.alpha = 0f;
            foreach (CanvasGroup group in reviveAlpha) group.alpha = 0f;
            background.color = Color.clear;

            foreach (Button b in buttons) b.interactable = false;
            foreach (Button b in buttonsRevive) b.interactable = false;
        }

        private void Start() {
            var result = FindObjectOfType<CrownParticleSign>();
            if(result == null){
                result = (Instantiate(Resources.Load("UIParticle/ParticleRenderCamera")) as GameObject).GetComponent<CrownParticleSign>();
            }
            system = result.particle;
        }

        internal void NormalPage(float percent, int blockCount, int crownCount)
        {
            progress = percent;
            ShowPage(true, percent, blockCount, crownCount);
        }

        internal void RevivePage(float percent)
        {
            progress = percent;
            ShowPage(false, percent);
        }

        internal void ShowPage(bool normal, float percent, int blockCount = 0, int crownCount = 0)
        {
            Ease movementCurve = Ease.InCubic;
            float movementY = 120F;
            Cursor.visible = true;
            var s = DOTween.Sequence();
            var CrownAniTime01 = 0.6f;
            var CrownAniTime02 = 1f;
            var CrownDisperseScale = 1f;
            if (normal)
            {
                moveUpPart.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutSine);
                moveDownPart.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutSine);
                background.DOFade(0.64f, 0.4f).SetEase(Ease.Linear).OnComplete(() => { foreach (Button b in buttons) b.interactable = true; });
                foreach (CanvasGroup c in normalAlpha) c.DOFade(1f, 0.4f).SetEase(Ease.Linear);
                barFill.sizeDelta = new Vector2(10f, 18f) + new Vector2(480f * percent, 0f);
                percentage.text = ((int)(percent * 100f)).ToString() + "%";
                block.text = $"{blockCount}/{player.levelData.MaxDiamondCount}";
                title.text = player.levelData.levelTitle;


                if (crownCount > 0)
                {

                    crownParticlesImage[0].color = fade;
                    crownParticlesImage[1].color = fade;
                    crownParticlesImage[2].color = fade;
                    system.Stop();

                    crownInfill[0].DOFade(1f, 0.6f).SetEase(Ease.InCirc);
                    (crownInfill[0].transform as RectTransform).anchoredPosition = new(-170, movementY);
                    (crownInfill[0].transform as RectTransform).DOAnchorPos(new(-150,0),0.6f).SetEase(movementCurve);
                    //crownInfill[0].GetComponentInChildren<RawImage>().DOFade(1f, 0.7f);
                    
                    
                    crownInfill[0].transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.InCirc).OnComplete(() =>
                    {
                        //crownInfill[0].GetComponentInChildren<RawImage>().DOFade(0, 0.3f);
                        crownParticlesImage[0].color = Color.white;
                        system.Play();
                        if (crownCount > 0) AudioSource.PlayClipAtPoint(crownSount[crownCount - 1], Camera.main.transform.position, 1f);
                        if (crownCount > 1)
                        {=
                            s.Append(crownInfill[1].DOFade(1f, CrownAniTime01).SetEase(Ease.Linear));
                            //s.Append(crownDisperse[1].transform.DOScale(CrownDisperseScale, 0f)).SetEase(Ease.Linear);
                            //s.Append(crownDisperse[1].DOFade(0f, CrownAniTime02).SetEase(Ease.Linear));
                            //s.Insert(CrownAniTime01, crownDisperse[1].transform.DOScale(new Vector3(m_Scale.x, m_Scale.y, 1), CrownAniTime02));
                            (crownInfill[1].transform as RectTransform).anchoredPosition = new(0, movementY);
                            (crownInfill[1].transform as RectTransform).DOAnchorPos(Vector2.zero, 0.6f).SetEase(movementCurve);
                            crownInfill[1].transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.InCubic).OnComplete(() =>
                            {
                                //crownInfill[1].GetComponentInChildren<RawImage>().DOFade(0, 0.3f);
                                crownParticlesImage[0].color = fade;
                                crownParticlesImage[1].color = Color.white;
                                system.Play();
                                if (crownCount > 2)
                                {
                                    crownInfill[2].DOFade(1f, 0.6f).SetEase(Ease.InCirc);
                                    (crownInfill[2].transform as RectTransform).anchoredPosition = new(170, movementY);
                                    (crownInfill[2].transform as RectTransform).DOAnchorPos(new(150,0),0.6f).SetEase(movementCurve);
                                    //crownInfill[2].GetComponentInChildren<RawImage>().DOFade(1f, 0.7f);
                                 
                                    crownInfill[2].transform.DOScale(Vector3.one, 0.6f).SetEase(Ease.InCirc).OnComplete(()=>{
                                        crownParticlesImage[1].color = fade;
                                        crownParticlesImage[2].color = Color.white;
                                        system.Play();
                                        
                                    });
                                }
                            });
                        }
                    });
                }


            }
            else
            {
                moveUpRevive.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutSine);
                moveDownRevive.DOAnchorPos(Vector2.zero, 0.4f).SetEase(Ease.OutSine);
                background.DOFade(0.64f, 0.4f).SetEase(Ease.Linear).OnComplete(() => { foreach (Button b in buttonsRevive) b.interactable = true; });
                foreach (CanvasGroup c in reviveAlpha) c.DOFade(1f, 0.4f).SetEase(Ease.Linear);
                barFillRevive.sizeDelta = new Vector2(10f, 18f) + new Vector2(480f * percent, 0f);
                percentageRevive.text = ((int)(percent * 100f)).ToString() + "%";
            }
        }

        public void ReloadScene()
        {
            foreach (Button b in buttons) b.interactable = false;
            if (LoadingPage.Instance) LoadingPage.Instance.Load(SceneManager.GetActiveScene().name);
            else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void RevivePlayer()
        {
            foreach (Button b in buttonsRevive) b.interactable = false;
            if (player.currentCheckpoint.GetComponent<Checkpoint>()) player.currentCheckpoint.GetComponent<Checkpoint>().Revival();
            else if (player.currentCheckpoint.GetComponent<Crown>()) player.currentCheckpoint.GetComponent<Crown>().Revival();
        }

        public void CancelRevive()
        {
            foreach (Button b in buttonsRevive) b.interactable = false;
            NormalPage(progress, player.BlockCount, player.CrownCount);

            moveUpRevive.DOAnchorPos(new Vector2(0f, -250f), 0.4f).SetEase(Ease.OutSine);
            moveDownRevive.DOAnchorPos(new Vector2(0f, 260f), 0.4f).SetEase(Ease.OutSine);
            foreach (CanvasGroup c in reviveAlpha) c.DOFade(0f, 0.4f).SetEase(Ease.Linear);
            foreach (CanvasGroup c in normalAlpha) c.DOFade(1f, 0.4f).SetEase(Ease.Linear);
        }

        internal void HideScreen(Color color, float duration, UnityAction fadeIn, UnityAction fadeOut)
        {
            foreach (Button b in buttons) b.interactable = false;
            foreach (Button b in buttonsRevive) b.interactable = false;

            hideScreenImage.color = new Color(color.r, color.g, color.b, 0f);
            hideScreenImage.DOFade(1f, duration).SetEase(Ease.Linear).OnComplete(() =>
            {
                ResetUI();
                fadeIn.Invoke();
                hideScreenImage.DOFade(0f, duration).SetEase(Ease.Linear).OnComplete(fadeOut.Invoke);
            });
        }

        private void ResetUI()
        {
            moveUpPart.anchoredPosition = new Vector2(0f, -250f);
            moveDownPart.anchoredPosition = new Vector2(0f, 430f);
            moveUpRevive.anchoredPosition = new Vector2(0f, -250f);
            moveDownRevive.anchoredPosition = new Vector2(0f, 260f);

            foreach (CanvasGroup group in normalAlpha) group.alpha = 0f;
            foreach (CanvasGroup group in reviveAlpha) group.alpha = 0f;
            background.color = Color.clear;

            foreach (Button b in buttons) b.interactable = false;
            foreach (Button b in buttonsRevive) b.interactable = false;
        }
    }
}