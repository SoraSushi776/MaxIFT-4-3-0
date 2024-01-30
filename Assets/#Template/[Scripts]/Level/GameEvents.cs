using UnityEngine;
using UnityEngine.Events;

namespace DancingLineFanmade.Level
{
    [DisallowMultipleComponent, RequireComponent(typeof(Player))]
    public class GameEvents : MonoBehaviour
    {
        [SerializeField] private UnityEvent onGameAwake;
        [SerializeField] private UnityEvent onPlayerStart;
        [SerializeField] private UnityEvent onChangeDirection;
        [SerializeField] private UnityEvent onLeaveGround;
        [SerializeField] private UnityEvent onTouchGround;
        [SerializeField] private UnityEvent onGameOver;
        [SerializeField] private UnityEvent onGetGem;
        [SerializeField] private UnityEvent onPlayerJump;

        public void Invoke(int index)
        {
            switch (index)
            {
                case 0: onGameAwake.Invoke(); break;
                case 1: onPlayerStart.Invoke(); break;
                case 2: onChangeDirection.Invoke(); break;
                case 3: onLeaveGround.Invoke(); break;
                case 4: onTouchGround.Invoke(); break;
                case 5: onGameOver.Invoke(); break;
                case 6: onGetGem.Invoke(); break;
                case 7: onPlayerJump.Invoke(); break;
                default: Debug.Log("Target event is not exist"); break;
            }
        }
    }
}