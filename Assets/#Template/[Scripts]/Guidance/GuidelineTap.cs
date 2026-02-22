using System.Collections;
using System.Collections.Generic;
using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Guideline
{
    [DisallowMultipleComponent]
    public class GuidelineTap : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Material material;
        [SerializeField] private Sprite sprite;
        [SerializeField] internal float displayTime = -100f;
        [SerializeField] internal float triggerTime;
        [SerializeField] internal float triggerDistance = 1f;
        [SerializeField] internal int colorIndex = 0;
        [SerializeField] internal bool haveLine = true;
        [SerializeField] internal bool triggered;

        private GameObject triggerEffect;
        private BoxCollider autoplayCollider;
        private readonly List<SpriteRenderer> sprites = new();
        private const float timeOffset = 0.25f;

        internal bool autoplay;
        internal bool noEffect;

        private float Distance => (transform.position - Player.Instance.transform.position).sqrMagnitude;

        public void SetColor(List<Color> colors)
        {
            spriteRenderer.color = colors[colorIndex];
        }

        private void Update()
        {
            if (AudioManager.Time > displayTime && !spriteRenderer.enabled && !triggered &&
                LevelManager.GameState == GameStatus.Playing)
                SetDisplay(true);
        }

        private void Start()
        {
            LevelManager.revivePlayer += revivePlayer;
        }

        private void revivePlayer()
        {
            triggered = false;
            if (!autoplay)
                Player.Instance.OnTurn.AddListener(Trigger);
            SetDisplay(false);
        }

        public void InitBox(bool auto)
        {
            if (!autoplay)
                Player.Instance.OnTurn.AddListener(Trigger);
            if (sprites.Count <= 0)
                sprites.AddRange(GetComponentsInChildren<SpriteRenderer>());
            if (triggerEffect == null)
                triggerEffect = Resources.Load<GameObject>("Prefabs/GuidelineTapEffect");
            spriteRenderer.material = material;
            spriteRenderer.sprite = sprite;
            autoplay = auto;
            noEffect = auto;
            if (displayTime <= 0)
            {
                displayTime = 0f;
                SetDisplay(true);
            }
            else SetDisplay(false);
        }

        public void AddBoxCollider(Vector3 size)
        {
            autoplayCollider = gameObject.AddComponent<BoxCollider>();
            autoplayCollider.isTrigger = true;
            autoplayCollider.size = size;
        }

        private void Trigger()
        {
            if (!(Distance <= triggerDistance) || !(Mathf.Abs(AudioManager.Time - triggerTime) <= timeOffset) ||
                triggered)
                return;
            triggered = true;
            if (noEffect)
                return;
            SetDisplay(false);
            StartCoroutine(DisplayEffect());
            Player.Instance.OnTurn.RemoveListener(Trigger);
        }

        public void SetDisplay(bool active)
        {
            foreach (var VARIABLE in sprites)
            {
                if (VARIABLE == null)
                    continue;
                VARIABLE.enabled = active;
            }
        }

        public IEnumerator DisplayEffect()
        {
            var color = Color.white;
            var scale = transform.localScale;
            var scaleVector = Vector3.one * 1.05f;
            var effect = Instantiate(triggerEffect, transform.position, Quaternion.Euler(-90, 0, 0)).transform;
            var component = effect.GetComponent<SpriteRenderer>();
            while (color.a > 0f)
            {
                yield return new WaitForSeconds(0.01f);
                color.a -= 0.03f;
                scale.Scale(scaleVector);
                component.color = color;
                effect.localScale = scale;
            }

            Destroy(effect.gameObject);
            yield return null;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!autoplay || autoplayCollider == null || !other.CompareTag("Player") || triggered ||
                Player.Instance.Falling)
                return;
            var playerPosition = Player.Instance.transform.position;
            var position = transform.position;
            var normalizedPlayerPosition = new Vector3(playerPosition.x, 0, playerPosition.z);
            var normalizedPosition = new Vector3(position.x, 0, position.z);
            var time = Mathf.Abs((normalizedPlayerPosition - normalizedPosition).magnitude) / Player.Instance.Speed;
            if (time > 0)
                Invoke(nameof(TurnPlayer), time);
            else TurnPlayer();
        }

        private void TurnPlayer()
        {
            Player.Instance.Turn();
        }
    }
}