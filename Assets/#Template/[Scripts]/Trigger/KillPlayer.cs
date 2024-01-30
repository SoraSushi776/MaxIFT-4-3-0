using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    public enum DieReason
    {
        Hit,
        Drowned,
        Border
    }

    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class KillPlayer : MonoBehaviour
    {
        private Player player;

        [SerializeField, EnumToggleButtons] private DieReason reason = DieReason.Drowned;

        private void Start()
        {
            player = Player.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !player.noDeath && LevelManager.GameState == GameStatus.Playing)
            {
                if (player.Checkpoints.Count <= 0) LevelManager.PlayerDeath(player, reason, Resources.Load<GameObject>("Prefabs/Remain"), null, false);
                else LevelManager.PlayerDeath(player, reason, Resources.Load<GameObject>("Prefabs/Remain"), null, true);
            }
        }
    }
}