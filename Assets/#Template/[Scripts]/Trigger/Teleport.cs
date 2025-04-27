#if UNITY_EDITOR
using UnityEditor;
#endif
using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    internal enum TeleportType
    {
        Target,
        Position
    }

    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class Teleport : MonoBehaviour
    {
        [SerializeField, EnumToggleButtons] private TeleportType type = TeleportType.Target;

        [SerializeField, HideIf("type", TeleportType.Position)]
        private Transform target;

        [SerializeField, HideIf("type", TeleportType.Target)]
        private Vector3 position = Vector3.zero;

        [SerializeField] private bool forceCameraFollow = true;
        [SerializeField] private bool turn;

        [SerializeField, ShowIf("turn"), EnumToggleButtons]
        private Direction targetDirection = Direction.First;

        private void OnTriggerEnter(Collider other)
        {
            var final = type switch
            {
                TeleportType.Target => target.position,
                TeleportType.Position => position,
                _ => Vector3.zero
            };
            if (other.CompareTag("Player"))
                LevelManager.SetPlayerPosition(Player.Instance, final, turn, targetDirection, forceCameraFollow);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 wirePosition;
            switch (type)
            {
                case TeleportType.Position:
                    wirePosition = position;
                    break;
                case TeleportType.Target when target == null:
                    Debug.LogError("传送目标点物体未选择。");
                    return;
                case TeleportType.Target:
                    wirePosition = target.position;
                    break;
                default:
                    wirePosition = Vector3.zero;
                    break;
            }

            var style = LevelManager.GUIStyle(new Color(0f, 0f, 0f, 0.6f), Color.white, 20);

            Handles.color = Color.white;
            Handles.DrawWireCube(wirePosition, Vector3.one);
            Handles.Label(wirePosition, gameObject.name, style);
            
            Handles.color = Color.red;
            Handles.DrawLine(wirePosition, transform.position, 2f);
        }
#endif
    }
}