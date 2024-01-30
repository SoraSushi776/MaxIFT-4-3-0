using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Guidance
{
    [DisallowMultipleComponent]
    public class GuidanceBox : MonoBehaviour
    {
        private Transform playerTransform;
        private Transform selfTransform;

        [SerializeField] private float triggerDistance = 1f;
        [SerializeField] private float appearDistance = 600f;
        [SerializeField] internal bool canBeTriggered = true;
        [SerializeField] internal bool haveLine = true;

        private SpriteRenderer spriteRenderer;
        private GameObject triggerEffect;
        private int index;

        internal bool triggered = false;
        internal bool displayed = false;

        public SpriteRenderer Renderer
        {
            get
            {
                if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
                return spriteRenderer;
            }
        }

        private float Distance
        {
            get => (selfTransform.position - playerTransform.position).sqrMagnitude;
        }

        public void SetColor(Color color)
        {
            Renderer.color = color;
        }

        private void Start()
        {
            playerTransform = Player.Instance.transform;
            selfTransform = transform;

            triggerEffect = Resources.Load<GameObject>("Prefabs/Triggered");
            if (Distance > appearDistance) Disappear(false);
        }

        private void Update()
        {
            if (!triggered && Distance <= appearDistance && !Renderer.enabled) Appear();
            if (LevelManager.Clicked && !triggered && Distance <= triggerDistance && canBeTriggered && LevelManager.GameState == GameStatus.Playing && !Player.Instance.disallowInput)
                Trigger();
        }

        private void Trigger()
        {
            triggered = true;
            Disappear(true);
            Destroy(Instantiate(triggerEffect, selfTransform.position, Quaternion.Euler(Vector3.zero)), 1f);
        }

        internal void Appear()
        {
            if (!displayed)
            {
                displayed = true;
                index = Player.Instance.Checkpoints.Count;

                SpriteRenderer[] renderers = selfTransform.GetComponentsInChildren<SpriteRenderer>();
                foreach (SpriteRenderer r in renderers) r.enabled = true;
                Renderer.enabled = true;

                LevelManager.revivePlayer += ResetData;
            }
        }

        internal void Disappear(bool onlyBox)
        {
            SpriteRenderer[] renderers = selfTransform.GetComponentsInChildren<SpriteRenderer>();
            if (!onlyBox)
            {
                foreach (SpriteRenderer r in renderers) r.enabled = false;
                Renderer.enabled = false;
            }
            else Renderer.enabled = false;
        }

        private void ResetData()
        {
            LevelManager.revivePlayer -= ResetData;
            displayed = false;
            triggered = false;
            Disappear(false);
        }

        private void OnDestroy()
        {
            LevelManager.revivePlayer -= ResetData;
        }
    }
}