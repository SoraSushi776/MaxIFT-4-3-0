using DancingLineFanmade.Trigger;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace DancingLineFanmade.Level
{
    public enum FakePlayerState
    {
        Moving,
        Stopped
    }

    [Serializable]
    public class ResetFakePlayer
    {
        public bool played;
        public int speed;
        public Vector3 position;
        public Vector3 rotation;

        public void GetData(FakePlayer player)
        {
            played = player.playing;
            speed = player.speed;
            position = player.transform.position;
            rotation = player.transform.eulerAngles;
        }

        public void SetData(FakePlayer player)
        {
            player.playing = played;
            player.speed = speed;
            player.transform.position = position;
            player.transform.eulerAngles = rotation;
            player.ClearPool();
            player.CreateTail();
        }
    }

    [DisallowMultipleComponent, RequireComponent(typeof(BoxCollider), typeof(Rigidbody))]
    public class FakePlayer : MonoBehaviour
    {
        public Rigidbody Rigidbody { get; private set; }
        public FakePlayerState state { get; set; }
        public bool playing { get; set; }

        [MinValue(0)] public int speed = 12;
        public Material characterMaterial;
        public Vector3 startPosition = Vector3.zero;
        public Vector3 firstDirection = new Vector3(0, 90, 0);
        public Vector3 secondDirection = Vector3.zero;
        [MinValue(1)] public int poolSize = 100;
        public bool isWall = false;
        public bool drawDirection = false;

        [SerializeField] private bool createTurnTrigger = true;
        [SerializeField, ShowIf("@createTurnTrigger")] private bool synchronismWithPlayer = false;
        [SerializeField, ShowIf("@createTurnTrigger && !synchronismWithPlayer")] private KeyCode createKey = KeyCode.P;
        [SerializeField, ShowIf("@createTurnTrigger && !synchronismWithPlayer")] private Vector3 triggerRotation = Vector3.zero;
        [SerializeField, ShowIf("@createTurnTrigger && !synchronismWithPlayer")] private Vector3 triggerScale = Vector3.one;
        private Transform triggerHolder;
        private int id = 0;

        private Transform selfTransform;
        private GameObject tailPrefab;
        private GameObject dustParticle;

        private BoxCollider characterCollider;
        private Vector3 tailPosition;
        private Transform tail;
        private Transform tailHolder;
        private ObjectPool<Transform> tailPool = new ObjectPool<Transform>();
        private ResetFakePlayer reset = new ResetFakePlayer();

        private float TailDistance
        {
            get => new Vector2(tailPosition.x - selfTransform.position.x, tailPosition.z - selfTransform.position.z).magnitude;
        }

        private bool previousFrameIsGrounded;
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

        private void Awake()
        {
            selfTransform = transform;
            Rigidbody = GetComponent<Rigidbody>();
            playing = false;
            tailHolder = new GameObject(gameObject.name + "-TailHolder").transform;

            characterCollider = GetComponent<BoxCollider>();
            groundedTestRays = new ValueTuple<Vector3, Ray>[]
            {
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * 0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * 0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down)),
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * -0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * 0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down)),
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * 0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * -0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down)),
                new ValueTuple<Vector3, Ray>(characterCollider.center - new Vector3(characterCollider.size.x * -0.5f, characterCollider.size.y * 0.5f - 0.1f, characterCollider.size.z * -0.5f), new Ray(Vector3.zero, selfTransform.localRotation * Vector3.down))
            };
            previousFrameIsGrounded = Falling;

            if (createTurnTrigger) triggerHolder = new GameObject("FakePlayerTriggerHolder").transform;
        }

        private void Start()
        {
            tailPool.Size = poolSize;
            firstDirection = firstDirection.Convert();
            secondDirection = secondDirection.Convert();
            selfTransform.position = startPosition;
            selfTransform.eulerAngles = firstDirection;
            tailPrefab = Instantiate(Resources.Load<GameObject>("Prefabs/FakeTail"), selfTransform);
            dustParticle = Resources.Load<GameObject>("Prefabs/Dust");

            selfTransform.GetComponent<MeshRenderer>().material = characterMaterial;
            tailPrefab.GetComponent<MeshRenderer>().material = characterMaterial;
            if (isWall)
            {
                gameObject.tag = "Obstacle";
                tailPrefab.tag = "Obstacle";
            }
            else
            {
                gameObject.tag = "FakePlayer";
                tailPrefab.tag = "FakePlayer";
            }
            state = FakePlayerState.Stopped;

            CreateTail();
        }

        private void Update()
        {
            switch (state)
            {
                case FakePlayerState.Moving:
                    selfTransform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
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
                        if (Falling) tail = null;
                        else
                        {
                            CreateTail();
                            Destroy(Instantiate(dustParticle, new Vector3(selfTransform.localPosition.x, selfTransform.localPosition.y - selfTransform.lossyScale.y * 0.5f + 0.2f, selfTransform.localPosition.z), Quaternion.Euler(90f, 0f, 0f)), 2f);
                        }
                    }
                    if (LevelManager.GameState == GameStatus.Died || LevelManager.GameState == GameStatus.Moving) state = FakePlayerState.Stopped;
#if UNITY_EDITOR
                    if (!synchronismWithPlayer)
                    {
                        if (Input.GetKeyDown(createKey)) CreateTriggers();
                    }
                    else
                    {
                        if (LevelManager.Clicked) CreateTriggers();
                    }
#endif
                    break;
            }
        }

        internal void Turn()
        {
            selfTransform.eulerAngles = selfTransform.eulerAngles == firstDirection ? secondDirection : firstDirection;
            CreateTail();
        }

        internal void CreateTail()
        {
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

        internal void GetData()
        {
            reset.GetData(this);
        }

        internal void ResetState()
        {
            state = FakePlayerState.Stopped;
            reset.SetData(this);
        }

        internal void ClearPool()
        {
            tailPool.DestoryAll();
            tail = null;
        }

        private void CreateTriggers()
        {
            GameObject g = LevelManager.CreateTrigger(selfTransform.position, triggerRotation, triggerScale, false, "FakePlayerTurnTrigger " + id);
            id++;
            FakePlayerTrigger f = g.AddComponent<FakePlayerTrigger>();
            g.transform.parent = triggerHolder;
            f.targetPlayer = this;
            f.type = SetType.Turn;
        }

        private void OnDrawGizmos()
        {
            if (drawDirection) LevelManager.DrawDirection(transform, 4);
        }

        [Button("Get Start Position", ButtonSizes.Large)]
        private void GetStartPosition()
        {
            startPosition = transform.position;
        }
    }
}