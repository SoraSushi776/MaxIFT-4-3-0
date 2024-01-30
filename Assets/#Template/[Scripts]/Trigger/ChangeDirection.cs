using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    public enum ChangeType
    {
        Direction,
        Turn
    }

    [RequireComponent(typeof(Collider))]
    public class ChangeDirection : MonoBehaviour
    {
        [SerializeField, EnumToggleButtons] private ChangeType type = ChangeType.Direction;

        [SerializeField, ShowIf("@type == ChangeType.Direction")] private Vector3 firstDirection = new Vector3(0, 90, 0);
        [SerializeField, ShowIf("@type == ChangeType.Direction")] private Vector3 secondDirection = Vector3.zero;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                switch (type)
                {
                    case ChangeType.Direction:
                        Player.Instance.firstDirection = firstDirection;
                        Player.Instance.secondDirection = secondDirection;
                        break;
                    case ChangeType.Turn: Player.Instance.Turn(); break;
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (type == ChangeType.Direction)
            {
                LevelManager.DrawDirection(transform, 3);

                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(transform.position, Vector3.one);
            }
        }
    }
}