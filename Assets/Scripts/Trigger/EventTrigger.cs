using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent]
    public class EventTrigger : MonoBehaviour
    {
        [SerializeField] private bool invokeOnAwake = false;
        [SerializeField, HideIf("invokeOnAwake")] private bool invokeOnClick = false;
        [SerializeField] private UnityEvent onTriggerEnter = new UnityEvent();

        private Player player;
        private bool invoked = false;
        private int index;

        private void Start()
        {
            player = Player.Instance;
            if (invokeOnAwake)
            {
                Invoke();
                invoked = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !invokeOnAwake && !invoked)
            {
                if (!invokeOnClick) Invoke(); else player.OnTurn.AddListener(Invoke);
                index = player.Checkpoints.Count;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player") && !invokeOnAwake && invokeOnClick) player.OnTurn.RemoveListener(Invoke);
        }

        private void Invoke()
        {
            if (!invoked)
            {
                onTriggerEnter.Invoke();
                invoked = true;
                LevelManager.revivePlayer += ResetData;
            }
        }

        private void ResetData()
        {
            LevelManager.revivePlayer -= ResetData;
            LevelManager.CompareCheckpointIndex(index, () =>
            {
                invoked = false;
                player.OnTurn.RemoveListener(Invoke);
            });
        }

        private void OnDestroy()
        {
            LevelManager.revivePlayer -= ResetData;
        }
    }
}