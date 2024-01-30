using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    public enum SetType
    {
        Turn,
        ChangeDirection,
        SetState
    }

    [RequireComponent(typeof(Collider))]
    public class FakePlayerTrigger : MonoBehaviour
    {
        [SerializeField] internal FakePlayer targetPlayer;
        [SerializeField, EnumToggleButtons] internal SetType type = SetType.Turn;
        [SerializeField, ShowIf("type", SetType.ChangeDirection)] private Vector3 firstDirection = new Vector3(0, 90, 0);
        [SerializeField, ShowIf("type", SetType.ChangeDirection)] private Vector3 secondDirection = Vector3.zero;
        [SerializeField, ShowIf("type", SetType.SetState)] private FakePlayerState state = FakePlayerState.Moving;

        private bool used = false;
        private int index;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                switch (type)
                {
                    case SetType.ChangeDirection:
                        targetPlayer.firstDirection = firstDirection;
                        targetPlayer.secondDirection = secondDirection;
                        break;
                    case SetType.SetState:
                        targetPlayer.state = state;
                        switch (state)
                        {
                            case FakePlayerState.Moving: targetPlayer.playing = true; break;
                            case FakePlayerState.Stopped: targetPlayer.playing = false; break;
                        }
                        break;
                }
            }
            if (other.CompareTag("FakePlayer") || other.CompareTag("Obstacle"))
            {
                switch (type)
                {
                    case SetType.Turn:
                        if (!used)
                        {
                            index = Player.Instance.Checkpoints.Count;
                            LevelManager.revivePlayer += ResetData;
                            targetPlayer?.Turn();
                            used = true;
                        }
                        break;
                }
            }
        }

        private void ResetData()
        {
            LevelManager.revivePlayer -= ResetData;
            LevelManager.CompareCheckpointIndex(index, () => used = false);
        }

        private void OnDestroy()
        {
            LevelManager.revivePlayer -= ResetData;
        }

        private void OnDrawGizmos()
        {
            if (type == SetType.ChangeDirection)
            {
                LevelManager.DrawDirection(transform, 3);

                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
        }
    }
}