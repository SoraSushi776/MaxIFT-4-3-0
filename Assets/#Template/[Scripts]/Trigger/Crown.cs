using DancingLineFanmade.Level;
using DancingLineFanmade.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Trigger
{
    public class Crown : MonoBehaviour
    {
        public GameObject crownObject;

        public ParticleSystem crownAura;

        public SpriteRenderer crownRenderer;

        private const float auraTweenDuration = 1.25f;

        private MeshRenderer crownMeshRenderer;

        private Tween crownTween;

        private readonly List<Tween> auraTweens = new();

        private bool taken;

        private Player player;
        private bool usedRevive;

        [Title("Player")] [SerializeField] private Direction direction = Direction.First;

        [SerializeField, HorizontalGroup("Camera")]
        private new CameraSettings camera = new CameraSettings();

        [SerializeField, HorizontalGroup("Camera"), HideLabel]
        private bool manualCamera = false;

        [SerializeField, HorizontalGroup("Fog")]
        private FogSettings fog = new FogSettings();

        [SerializeField, HorizontalGroup("Fog"), HideLabel]
        private bool manualFog = false;

        [SerializeField, HorizontalGroup("Light")]
        private new LightSettings light = new LightSettings();

        [SerializeField, HorizontalGroup("Light"), HideLabel]
        private bool manualLight = false;

        [SerializeField, HorizontalGroup("Ambient")]
        private AmbientSettings ambient = new AmbientSettings();

        [SerializeField, HorizontalGroup("Ambient"), HideLabel]
        private bool manualAmbient = false;

        [Title("Colors")] [SerializeField, TableList]
        private List<SingleColor> materialColorsAuto = new List<SingleColor>();

        [SerializeField, TableList] private List<SingleColor> materialColorsManual = new List<SingleColor>();

        [SerializeField, TableList] private List<SingleImage> imageColorsAuto = new List<SingleImage>();
        [SerializeField, TableList] private List<SingleImage> imageColorsManual = new List<SingleImage>();

        [Title("Event")] [SerializeField] private UnityEvent onRevive = new UnityEvent();

        [Space(30.0f), SerializeField] private bool AutoRecord = false;

        [SerializeField, HideIf(nameof(AutoRecord))]
        private float GameTime;

        private int trackProgress;

        [SerializeField, HideIf(nameof(AutoRecord))]
        private float playerSpeed;

        public UnityEvent eventWeNeedInvokeWhenJumpToHere;

        private Vector3 sceneGravity;
        private Vector3 playerFirstDirection;
        private Vector3 playerSecondDirection;

        public Transform revivePosition;

        private List<SetActive> actives = new List<SetActive>();
        private List<PlayAnimator> animators = new List<PlayAnimator>();
        private List<FakePlayer> fakes = new List<FakePlayer>();

        private void Start()
        {
            player = Player.Instance;
            actives = FindObjectsOfType<SetActive>(true).ToList();
            animators = FindObjectsOfType<PlayAnimator>(true).ToList();
            fakes = FindObjectsOfType<FakePlayer>(true).ToList();
            crownMeshRenderer = crownObject.GetComponent<MeshRenderer>();
            InitParticles();
            if (crownAura) crownAura.transform.parent = transform;
        }

        private void Update()
        {
            crownObject.transform.Rotate(Vector3.up, Time.deltaTime * 40f, Space.Self);
        }

        private void InitParticles()
        {
            if (!crownAura) return;
            var color = crownMeshRenderer.material.color;
            color.a = 0f;
            var systems = crownAura.GetComponentsInChildren<ParticleSystem>();
            foreach (var VARIABLE in systems)
            {
                var main = VARIABLE.main;
                main.startColor = color;
            }

            crownAura.Play();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") || taken) return;
            taken = true;

            player.Crowns.Add(this);
            player.CrownCount++;
            player.currentCheckpoint = this;
            player.lastCrown = this;

            if (!manualCamera && CameraFollower.Instance) camera = camera.GetCamera();
            if (!manualFog) fog = fog.GetFog();
            if (!manualLight) light = light.GetLight(player.sceneLight);
            if (!manualAmbient) ambient = ambient.GetAmbient();
            foreach (SingleColor s in materialColorsAuto) s.GetColor();
            foreach (SingleImage s in imageColorsAuto) s.GetColor();

            if (AutoRecord)
            {
                GameTime = AudioManager.Time;
                playerSpeed = player.Speed;
            }

            playerSpeed = player.Speed;
            sceneGravity = Physics.gravity;
            playerFirstDirection = player.firstDirection;
            playerSecondDirection = player.secondDirection;

            foreach (SetActive s in actives)
                if (!s.activeOnAwake)
                    s.AddRevives();
            foreach (PlayAnimator a in animators)
            foreach (SingleAnimator s in a.animators)
                if (!s.dontRevive)
                    s.GetState();
            foreach (FakePlayer f in fakes) f.GetData();
            player.GetAnimatorProgresses();
            player.GetTimelineProgresses();
            TakeCrown();
        }

        private void TakeCrown()
        {
            RefreshParticlesColor();
            StopAnimations();
            crownAura.transform.position = crownObject.transform.position;
            crownAura.Play();
            var pos = crownRenderer.transform.position;
            auraTweens.Add(crownAura.transform.DOMoveX(pos.x, auraTweenDuration));
            auraTweens[^1].SetEase(Ease.InOutSine);
            auraTweens.Add(crownAura.transform.DOMoveZ(pos.z, auraTweenDuration));
            auraTweens[^1].SetEase(Ease.InOutSine);
            auraTweens.Add(crownAura.transform.DOMoveY(pos.y + 5f,
                auraTweenDuration / 2f));
            auraTweens[^1].SetEase(Ease.InSine);
            Tweener tween = crownAura.transform.DOMoveY(pos.y, auraTweenDuration / 2f);
            tween.SetEase(Ease.OutSine);
            tween.SetDelay(auraTweenDuration / 2f);
            tween.OnStart(ShowSpirit);
            auraTweens.Add(tween);
            auraTweens[0].OnComplete(ClearAuraTweens);
            crownMeshRenderer.enabled = false;
        }

        private void ShowSpirit()
        {
            AnimateCrown(true);
        }

        private void AnimateCrown(bool show)
        {
            crownTween = crownRenderer.DOFade(show ? 1 : 0, auraTweenDuration / 4f);
            crownTween.SetEase(Ease.OutSine);
            crownTween.OnComplete(() =>
            {
                crownTween.Kill();
                crownTween = null;
            });
        }

        private void RefreshParticlesColor()
        {
            var color = crownMeshRenderer.material.color;
            var systems = crownAura.GetComponentsInChildren<ParticleSystem>();
            foreach (var VARIABLE in systems)
            {
                var main = VARIABLE.main;
                main.startColor = color;
            }
        }

        private void StopAnimations()
        {
            ClearAuraTweens();
            if (crownTween == null) return;
            crownTween.Kill();
            crownTween = null;
        }

        private void ClearAuraTweens()
        {
            foreach (var VARIABLE in auraTweens) VARIABLE.Kill();
            auraTweens.Clear();
        }

        internal void Revival()
        {
            DOTween.Clear();
            LevelUI.Instance.HideScreen(fog.fogColor, 0.32f, () =>
                {
                    ResetScene();
                    LevelManager.revivePlayer.Invoke();
                    LevelManager.DestroyRemain();
                    Player.Rigidbody.isKinematic = true;
                    if (!usedRevive) Player.Instance.CrownCount--;
                    usedRevive = true;
                },
                () =>
                {
                    Player.Rigidbody.isKinematic = false;
                    player.allowTurn = true;
                });
            AnimateCrown(false);
        }

        private void ResetScene()
        {
            if (CameraFollower.Instance) camera.SetCamera();
            fog.SetFog(player.sceneCamera);
            light.SetLight(player.sceneLight);
            ambient.SetAmbient();
            foreach (SingleColor s in materialColorsAuto) s.SetColor();
            foreach (SingleColor s in materialColorsManual) s.SetColor();
            foreach (SingleImage s in imageColorsAuto) s.SetColor();
            foreach (SingleImage s in imageColorsManual) s.SetColor();

            AudioManager.Stop();
            AudioManager.Time = GameTime;
            AudioManager.Volume = 1f;
            player.SoundTrackProgress = trackProgress;
            player.ClearPool();
            player.BlockCount = 0;
            player.Speed = playerSpeed;
            Physics.gravity = sceneGravity;
            player.firstDirection = playerFirstDirection;
            player.secondDirection = playerSecondDirection;
            LevelManager.InitPlayerPosition(player, revivePosition.position, true, direction);
            foreach (SetActive s in actives)
                if (!s.activeOnAwake)
                    s.Revive();
            foreach (PlayAnimator a in animators)
            foreach (SingleAnimator s in a.animators)
                if (!s.dontRevive && s.played)
                    s.SetState();
            foreach (FakePlayer f in fakes)
                if (f.playing)
                    f.ResetState();
            player.SetAnimatorProgresses(GameTime);
            player.SetTimelineProgresses(GameTime);

            onRevive.Invoke();
        }

        public void JumpToHere()
        {
            if (LevelManager.GameState == GameStatus.Waiting)
            {
                LevelUI.Instance.HideScreen(fog.fogColor, 0.32f, () =>
                    {
                        LevelManager.InitPlayerPosition(player, revivePosition.position, true, direction);
                        player.SoundTrack = AudioManager.PlayTrack(player.levelData.soundTrack, 1, false);
                        player.SoundTrack.time = GameTime;
                        player.SetTimelineProgresses(GameTime);
                        player.SetAnimatorProgresses(GameTime);
                        eventWeNeedInvokeWhenJumpToHere.Invoke();
                        LevelManager.InitPlayerPosition(player, revivePosition.position, true, direction);
                    },
                    () =>
                    {
                        Player.Rigidbody.isKinematic = false;
                        player.allowTurn = true;
                    });
            }
        }
    }
}