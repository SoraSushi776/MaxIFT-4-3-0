using DancingLineFanmade.Level;
using DancingLineFanmade.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent]
    public class Checkpoint : MonoBehaviour
    {
        private Player player;

        private Transform rotator;
        private Transform frame;
        private Transform core;
        private Transform revivePosition;

        [Title("Player")]
        [SerializeField] private Direction direction = Direction.First;

        [SerializeField, HorizontalGroup("Camera")] private new CameraSettings camera = new CameraSettings();
        [SerializeField, HorizontalGroup("Camera"), HideLabel] private bool manualCamera = false;

        [SerializeField, HorizontalGroup("Fog")] private FogSettings fog = new FogSettings();
        [SerializeField, HorizontalGroup("Fog"), HideLabel] private bool manualFog = false;

        [SerializeField, HorizontalGroup("Light")] private new LightSettings light = new LightSettings();
        [SerializeField, HorizontalGroup("Light"), HideLabel] private bool manualLight = false;

        [SerializeField, HorizontalGroup("Ambient")] private AmbientSettings ambient = new AmbientSettings();
        [SerializeField, HorizontalGroup("Ambient"), HideLabel] private bool manualAmbient = false;

        [Title("Colors")]
        [SerializeField, TableList] private List<SingleColor> materialColorsAuto = new List<SingleColor>();
        [SerializeField, TableList] private List<SingleColor> materialColorsManual = new List<SingleColor>();

        [SerializeField, TableList] private List<SingleImage> imageColorsAuto = new List<SingleImage>();
        [SerializeField, TableList] private List<SingleImage> imageColorsManual = new List<SingleImage>();

        [Title("Event")]
        [SerializeField] private UnityEvent onRevive = new UnityEvent();

        private float trackTime;
        private int trackProgress;
        private int playerSpeed;
        private Vector3 sceneGravity;
        private Vector3 playerFirstDirection;
        private Vector3 playerSecondDirection;

        private List<SetActive> actives = new List<SetActive>();
        private List<PlayAnimator> animators = new List<PlayAnimator>();
        private List<FakePlayer> fakes = new List<FakePlayer>();

        private void Start()
        {
            player = Player.Instance;

            rotator = transform.Find("Rotator");
            frame = rotator.Find("Frame");
            core = rotator.Find("Core");
            revivePosition = transform.Find("RevivePosition");
            revivePosition.gameObject.SetActive(false);

            actives = FindObjectsOfType<SetActive>(true).ToList();
            animators = FindObjectsOfType<PlayAnimator>(true).ToList();
            fakes = FindObjectsOfType<FakePlayer>(true).ToList();
        }

        private void Update()
        {
            frame.Rotate(Vector3.up, Time.deltaTime * -45f);
            core.Rotate(Vector3.up, Time.deltaTime * 45f);
        }

        internal void EnterTrigger()
        {
            player.Checkpoints.Add(this);
            player.currentCheckpoint = this;
            LevelManager.CaculateAvailableCrowns(false,this);
            rotator.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

            if (!manualCamera && CameraFollower.Instance) camera = camera.GetCamera();
            if (!manualFog) fog = fog.GetFog();
            if (!manualLight) light = light.GetLight(player.sceneLight);
            if (!manualAmbient) ambient = ambient.GetAmbient();
            foreach (SingleColor s in materialColorsAuto) s.GetColor();
            foreach (SingleImage s in imageColorsAuto) s.GetColor();

            trackTime = AudioManager.Time;
            trackProgress = player.SoundTrackProgress;
            playerSpeed = player.Speed;
            sceneGravity = Physics.gravity;
            playerFirstDirection = player.firstDirection;
            playerSecondDirection = player.secondDirection;

            foreach (SetActive s in actives) if (!s.activeOnAwake) s.AddRevives();
            foreach (PlayAnimator a in animators) foreach (SingleAnimator s in a.animators) if (!s.dontRevive) s.GetState();
            foreach (FakePlayer f in fakes) f.GetData();
            player.GetAnimatorProgresses();
            player.GetTimelineProgresses();
        }

        internal void Revival()
        {
            DOTween.Clear();
            LevelUI.Instance.HideScreen(fog.fogColor, 0.32f, () =>
                {
                    ResetScene();
                    LevelManager.revivePlayer.Invoke();
                    LevelManager.CaculateAvailableCrowns(true,this);
                    LevelManager.DestroyRemain();
                    core.gameObject.SetActive(false);
                    Player.Rigidbody.isKinematic = true;
                },
                () =>
                {
                    Player.Rigidbody.isKinematic = false;
                    player.allowTurn = true;
                });
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
            AudioManager.Time = trackTime;
            AudioManager.Volume = 1f;
            player.SoundTrackProgress = trackProgress;
            player.ClearPool();
            player.BlockCount = 0;
            player.Speed = playerSpeed;
            Physics.gravity = sceneGravity;
            player.firstDirection = playerFirstDirection;
            player.secondDirection = playerSecondDirection;
            LevelManager.InitPlayerPosition(player, revivePosition.position, true, direction);
            foreach (SetActive s in actives) if (!s.activeOnAwake) s.Revive();
            foreach (PlayAnimator a in animators) foreach (SingleAnimator s in a.animators) if (!s.dontRevive && s.played) s.SetState();
            foreach (FakePlayer f in fakes) if (f.playing) f.ResetState();
            player.SetAnimatorProgresses();
            player.SetTimelineProgresses();

            onRevive.Invoke();
        }
    }
}