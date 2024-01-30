using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    enum TeleportType
    {
        Target,
        Position
    }

    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class Teleport : MonoBehaviour
    {
        [SerializeField, EnumToggleButtons] private TeleportType type = TeleportType.Target;
        [SerializeField, HideIf("type", TeleportType.Position)] private Transform target;
        [SerializeField, HideIf("type", TeleportType.Target)] private Vector3 position = Vector3.zero;

        [SerializeField] private bool turn = false;
        [SerializeField, ShowIf("turn")] private Direction targetDirection = Direction.First;

        private void OnTriggerEnter(Collider other)
        {
            Vector3 final;
            switch (type)
            {
                case TeleportType.Target: final = target.position; break;
                case TeleportType.Position: final = position; break;
                default: final = Vector3.zero; break;
            }
            if (other.CompareTag("Player")) LevelManager.InitPlayerPosition(Player.Instance, final, turn, targetDirection);
        }
    }
}