using DancingLineFanmade.Trigger;
using DancingLineFanmade.UI;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace DancingLineFanmade.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public class Player : MonoBehaviour
    {
        private Transform selfTransform;
        public static Rigidbody characterRigidbody;

        public static Player Instance { get; private set; }


        private GameObject tailPrefab;
        private GameObject cubesPrefab;
        private GameObject dustParticle;
        private GameObject uiPrefab;
        private GameObject startPrefab;
        private GameObject loadingPrefab;

        [Title("Data")]
        [Required("必须选填关卡数据文件")] public LevelData levelData;

        [Title("Settings")]
        public Camera sceneCamera;
        public Light sceneLight;
        public Material characterMaterial;

        public Vector3 startPosition = Vector3.zero;
        public Vector3 firstDirection = new Vector3(0, 90, 0);
        public Vector3 secondDirection = Vector3.zero;
        [MinValue(1)] public int poolSize = 100;
        public List<Animator> playedAnimators = new List<Animator>();
        public List<PlayableDirector> playedTimelines = new List<PlayableDirector>();
        public bool allowTurn = true;
        public bool noDeath = false;
        public bool drawDirection = false;

        internal float Speed { get; set; }
        internal AudioSource SoundTrack { get; set; }
        internal int LevelProgress { get; set; }
        internal int BlockCount { get; set; }
        internal int CrownCount { get; set; }
        internal UnityEvent OnTurn { get; private set; }
        internal List<Checkpoint> Checkpoints { get; set; }
        internal List<Crown> Crowns { get; set; }
        internal bool disallowInput { get; set; }
        internal StartPage startPage;

        private BoxCollider characterCollider;
        private Vector3 tailPosition;
        private Transform tailHolder;
        private ObjectPool<Transform> tailPool = new ObjectPool<Transform>();
        private List<float> animatorProgresses = new List<float>();
        private List<double> timelineProgresses = new List<double>();
        private bool debug = true;
        private bool loading = false;

        [HideInInspector] public Transform tail;
        [HideInInspector] public bool allowCreateTail = true;
        [HideInInspector] public Component currentCheckpoint;
        [HideInInspector] public Crown lastCrown;

        [HideInInspector] public Transform henshinObject;
        [HideInInspector] public Vector3 objectOffset;
        [HideInInspector] public bool showLineTail, showLineBody;
        [HideInInspector] public bool henShin = false;
        [HideInInspector] public float rotationTime;
        private bool didCreateTail = false;

        private float TailDistance
        {
            get => new Vector2(tailPosition.x - selfTransform.position.x, tailPosition.z - selfTransform.position.z).magnitude;
        }

        public bool previousFrameIsGrounded;
        private float groundedRayDistance = 0.05f;
        private ValueTuple<Vector3, Ray>[] groundedTestRays;
        private RaycastHit[] groundedTestResults = new RaycastHit[1];
        public bool Falling
        {
            get
            {
                for (int i = 0; i < groundedTestRays.Length; i++)
                {
                    groundedTestRays[i].Item2.origin = selfTransform.position + selfTransform.localRotation * groundedTestRays[i].Item1;
                    if (Physics.RaycastNonAlloc(groundedTestRays[i].Item2, groundedTestResults, groundedRayDistance + 0.1f, -257, QueryTriggerInteraction.Ignore) > 0)
                        return false;
                }
                return true;
            }
        }

        private int frame;
        private float lastTime;
        private float fps;
        private const float timeInterval = 0.1f;
        public float gameTime;

        private GameEvents events;
        public GameEvents Events
        {
            get => events ? events : (events = GetComponent<GameEvents>() ? GetComponent<GameEvents>() : null);
        }
        private void Awake()
        {
            if (!levelData)
            {
                Debug.LogError("无法获取关卡信息，请确保关卡数据文件（Level Data）填选正确且不为空");
                LevelManager.DialogBox("警告", "无法获取关卡信息，请确保关卡数据文件（Level Data）填选正确且不为空", "确定", true);
                return;
            }
            DOTween.Clear();
            DOTween.SetTweensCapacity(2000, 1000);
            Instance = this;
            characterRigidbody = GetComponent<Rigidbody>();
            loading = false;
            Checkpoints = new List<Checkpoint>();
            Crowns = new List<Crown>();
            OnTurn = new UnityEvent();
            selfTransform = transform;
            tailHolder = new GameObject("PlayerTailHolder").transform;
            disallowInput = false;

            characterCollider = GetComponent<BoxCollider>();
            groundedTestRays = new ValueTuple<Vector3, Ray>[]
            {
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * 0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * 0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down)),
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * -0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * 0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down)),
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * 0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * -0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down)),
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * -0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * -0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down))
            };
            previousFrameIsGrounded = Falling;

            foreach (Animator animator in playedAnimators) animator.speed = 0f;

            LoadingPage.Instance?.Fade(0f, 0.4f);

            lastTime = Time.realtimeSinceStartup;

            for (int a = 0; a < playedTimelines.Count; a++)
            {
                playedTimelines[a].time = 0f;
                playedTimelines[a].Pause();
                playedTimelines[a].Evaluate();
            }

            SoundTrack = AudioManager.PlayTrack(levelData.soundTrack, 1f, false);
        }

        private void Start()
        {
            levelData.SetLevelData();
            firstDirection = firstDirection.Convert();
            secondDirection = secondDirection.Convert();
            tailPool.Size = poolSize;
            LevelManager.InitPlayerPosition(this, startPosition, false);
            tailPrefab = Resources.Load<GameObject>("Prefabs/Tail");
            cubesPrefab = Resources.Load<GameObject>("Prefabs/Remain");
            dustParticle = Resources.Load<GameObject>("Prefabs/Dust");
            uiPrefab = Resources.Load<GameObject>("Prefabs/LevelUI");
            startPrefab = Resources.Load<GameObject>("Prefabs/StartPage");
            loadingPrefab = Resources.Load<GameObject>("Prefabs/LoadingPage");

            selfTransform.GetComponent<MeshRenderer>().material = characterMaterial;
            tailPrefab.GetComponent<MeshRenderer>().material = characterMaterial;
            selfTransform.eulerAngles = firstDirection;
            LevelManager.GameState = GameStatus.Waiting;
            Instantiate(uiPrefab);
            startPage = Instantiate(startPrefab).GetComponent<StartPage>();
            if (!LoadingPage.Instance) DontDestroyOnLoad(Instantiate(loadingPrefab));

            Player.Instance.OnTurn.AddListener(() =>
            {
                if (!henShin) return;
                DOTween.Kill(100);
                henshinObject.transform.DORotate(Player.Instance.transform.eulerAngles, rotationTime).SetId(100);
            });

        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.R) && !loading)
            {
                loading = true;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            if (Input.GetKeyDown(KeyCode.C) && LevelManager.GameState == GameStatus.Playing) Debug.Log("当前时间：" + AudioManager.Time);
            if (Input.GetKeyDown(KeyCode.D)) debug = !debug;
            if (Input.GetKeyDown(KeyCode.K) && LevelManager.GameState == GameStatus.Playing) LevelManager.PlayerDeath(this, DieReason.Hit, cubesPrefab, null, false);
            if (Input.GetKeyDown(KeyCode.S) && LevelManager.GameState == GameStatus.Playing) LevelManager.CreateTrigger(selfTransform.position, Vector3.zero, new Vector3(3, 3, 3), false, "CreatedTrigger");

            GetFrame();
#endif
            if (allowTurn && !LevelManager.IsPointedOnUI())
            {
                switch (LevelManager.GameState)
                {
                    case GameStatus.Waiting:
                        if (LevelManager.Clicked && !Falling)
                        {
                            LevelManager.GameState = GameStatus.Playing;
                            AudioManager.Play();
                            foreach (Animator a in playedAnimators) a.speed = 1f;
                            foreach (PlayableDirector p in playedTimelines) p.Play();
                            foreach (PlayAnimator p in FindObjectsOfType<PlayAnimator>(true)) foreach (SingleAnimator s in p.animators) if (s.played) s.PlayAnimator();
                            foreach (FakePlayer f in FindObjectsOfType<FakePlayer>(true)) if (f.playing) f.state = FakePlayerState.Moving;
                            CreateTail();
                            Events?.Invoke(1);
                            if (startPage)
                            {
                                startPage.Hide();
                                startPage = null;
                            }
                            if (!Application.isEditor) Cursor.visible = false;
                            if (currentCheckpoint != null)
                            {
                                if (currentCheckpoint.GetComponent<Crown>())
                                {
                                    currentCheckpoint.GetComponent<Crown>().AnimateCrown(false);
                                }
                            }
                        }
                        break;
                    case GameStatus.Playing:
                        if (LevelManager.Clicked && !Falling && !disallowInput) Turn();
                        gameTime += Time.deltaTime;
                        break;
                }
            }
            if (LevelManager.GameState == GameStatus.Playing || LevelManager.GameState == GameStatus.Moving)
            {
                selfTransform.Translate(Vector3.forward * Speed * Time.deltaTime, Space.Self);
                if (tail && !Falling)
                {
                    tail.position = (tailPosition + selfTransform.position) * 0.5f;
                    tail.localScale = new Vector3(tail.localScale.x, tail.localScale.y, TailDistance);
                    tail.position = new Vector3(tail.position.x, selfTransform.position.y, tail.position.z);
                    tail.LookAt(selfTransform);
                }
                if (previousFrameIsGrounded != Falling)
                {
                    previousFrameIsGrounded = Falling;
                    if (Falling)
                    {
                        tail = null;
                        Events?.Invoke(3);
                    }
                    else
                    {
                        CreateTail();
                        Destroy(Instantiate(dustParticle, new Vector3(selfTransform.localPosition.x, selfTransform.localPosition.y - selfTransform.lossyScale.y * 0.5f + 0.2f, selfTransform.localPosition.z), Quaternion.Euler(90f, 0f, 0f)), 2f);
                        Events?.Invoke(4);
                    }
                }
            }
            if (LevelManager.GameState == GameStatus.Playing)
            {
                if (levelData.useMusicTime)
                    LevelProgress = SoundTrack ? (int)(AudioManager.Progress * 100) : 0;
                else
                    LevelProgress = (int)(gameTime / levelData.levelTime * 100) >= 100 ? 100 : (int)(gameTime / levelData.levelTime * 100);
            }

            if (henShin)
            {
                didCreateTail = false;
                henshinObject.position = Player.Instance.transform.position + objectOffset;
                if (!showLineTail)
                {
                    Player.Instance.tail = null;
                    Player.Instance.allowCreateTail = false;
                }
                Player.Instance.GetComponent<MeshRenderer>().enabled = showLineBody;
            }
            else
            {
                if (!didCreateTail)
                {
                    Player.Instance.allowCreateTail = true;
                    Player.Instance.CreateTail();
                    Player.Instance.GetComponent<MeshRenderer>().enabled = true;
                    didCreateTail = true;
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.CompareTag("Obstacle") && !noDeath && LevelManager.GameState == GameStatus.Playing)
            {
                if (Checkpoints.Count <= 0 && Crowns.Count <= 0)
                {
                    LevelManager.PlayerDeath(this, DieReason.Hit, !showLineBody ? null : cubesPrefab, collision);
                }
                else
                {
                    if (Checkpoints.Count > 0)
                    {
                        LevelManager.PlayerDeath(this, DieReason.Hit, !showLineBody ? null : cubesPrefab, collision, true);
                    }
                    else if (Crowns.Count > 0)
                    {
                        LevelManager.PlayerDeath(this, DieReason.Hit, !showLineBody ? null : cubesPrefab, collision, true);
                    }
                }
            }
        }

        internal void Turn()
        {
            selfTransform.eulerAngles = selfTransform.eulerAngles == firstDirection ? secondDirection : firstDirection;
            CreateTail();
            OnTurn.Invoke();
            Events?.Invoke(2);
        }

        public void CreateTail()
        {
            if (!allowCreateTail) return;

            Quaternion now = Quaternion.Euler(selfTransform.localEulerAngles);
            float offset = tailPrefab.transform.localScale.z * 0.5f;

            if (tail)
            {
                Quaternion last = Quaternion.Euler(tail.transform.localEulerAngles);
                float angle = Quaternion.Angle(last, now);
                if (angle >= 0f && angle <= 90f) offset = 0.5f * Mathf.Tan(Mathf.PI / 180f * angle * 0.5f);
                else offset = -0.5f * Mathf.Tan(Mathf.PI / 180f * ((180f - angle) * 0.5f));
                Vector3 end = tailPosition + last * Vector3.forward * (TailDistance + offset);
                tail.position = (tailPosition + end) * 0.5f;
                tail.position = new Vector3(tail.position.x, selfTransform.position.y, tail.position.z);
                tail.localScale = new Vector3(tail.localScale.x, tail.localScale.y, Vector3.Distance(tailPosition, end));
                tail.LookAt(selfTransform.position);
            }
            tailPosition = selfTransform.position + now * Vector3.back * Mathf.Abs(offset);
            if (!tailPool.Full)
            {
                tail = Instantiate(tailPrefab, selfTransform.position, selfTransform.rotation).transform;
                tail.parent = tailHolder;
                tailPool.Add(tail);
            }
            else
            {
                tail = tailPool.First();
                tailPool.Add(tail);
            }
        }

        internal void ClearPool()
        {
            tailPool.DestoryAll();
            tail = null;
        }

        internal void GetAnimatorProgresses()
        {
            animatorProgresses.Clear();
            foreach (Animator a in playedAnimators) animatorProgresses.Add(a.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }

        internal void SetAnimatorProgresses(float progress = 0f)
        {
            for (int a = 0; a < playedAnimators.Count; a++)
            {
                if (progress != 0 && a == 0)
                {
                    playedAnimators[a].Play(playedAnimators[a].GetCurrentAnimatorStateInfo(0).fullPathHash, 0, progress);
                }
                else
                {
                    playedAnimators[a].Play(playedAnimators[a].GetCurrentAnimatorStateInfo(0).fullPathHash, 0, animatorProgresses[a]);
                }
            }
        }

        internal void GetTimelineProgresses()
        {
            timelineProgresses.Clear();
            foreach (PlayableDirector p in playedTimelines) timelineProgresses.Add(p.time);
        }

        internal void SetTimelineProgresses(float progress = 0f)
        {
            for (int a = 0; a < playedTimelines.Count; a++)
            {
                if (progress != 0 && a == 0)
                {
                    playedTimelines[a].time = progress;
                    playedTimelines[a].Evaluate();
                }
                else
                {
                    playedTimelines[a].time = timelineProgresses[a];
                    playedTimelines[a].Evaluate();
                }
            }
        }

#if UNITY_EDITOR
        private void GetFrame()
        {
            frame++;
            if (Time.realtimeSinceStartup - lastTime < timeInterval) return;

            float time = Time.realtimeSinceStartup - lastTime;
            fps = frame / time;

            lastTime = Time.realtimeSinceStartup;
            frame = 0;
        }

        private void OnGUI()
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = LevelManager.GetColorByContent(sceneCamera.backgroundColor);
            style.fontSize = 25;

            int finalFps = fps > 999f ? 999 : (int)fps;
            if (debug)
            {
                GUI.Label(new Rect(10, 10, 120, 50), "FPS：" + finalFps, style);
                GUI.Label(new Rect(10, 40, 120, 50), "关卡进度：" + LevelProgress + "%", style);
                GUI.Label(new Rect(10, 70, 120, 50), "游戏状态：" + LevelManager.GameState, style);
                GUI.Label(new Rect(10, 100, 120, 50), "线的坐标：" + selfTransform.localPosition, style);
                GUI.Label(new Rect(10, 130, 120, 50), "线的朝向：" + selfTransform.localEulerAngles, style);
                GUI.Label(new Rect(10, 160, 120, 50), "已获取方块数量：" + BlockCount + "/" + levelData.MaxDiamondCount, style);
                GUI.Label(new Rect(10, 190, 120, 50), "已获取皇冠数量：" + CrownCount + "/3", style);
                if (CameraFollower.Instance)
                {
                    GUI.Label(new Rect(10, 220, 120, 50), "相机偏移：" + CameraFollower.Instance.rotator.localPosition, style);
                    GUI.Label(new Rect(10, 250, 120, 50), "相机角度：" + CameraFollower.Instance.rotator.localEulerAngles, style);
                    GUI.Label(new Rect(10, 280, 120, 50), "相机缩放：" + CameraFollower.Instance.scale.localScale, style);
                    GUI.Label(new Rect(10, 310, 120, 50), "视场大小：" + sceneCamera.fieldOfView, style);
                }
                else
                {
                    GUI.Label(new Rect(10, 220, 120, 50), "相机位置：" + sceneCamera.transform.position, style);
                    GUI.Label(new Rect(10, 250, 120, 50), "相机角度：" + sceneCamera.transform.eulerAngles, style);
                    GUI.Label(new Rect(10, 280, 120, 50), "视场大小：" + sceneCamera.fieldOfView, style);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (drawDirection) LevelManager.DrawDirection(transform, 4, 1);
        }

        [Button("Get Start Position", ButtonSizes.Large)]
        private void GetStartPosition()
        {
            startPosition = transform.position;
        }
#endif
    }
}