using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Auto
{
    [DisallowMultipleComponent]
    public class SetAutoPlay : MonoBehaviour
    {
        private bool active = false;

        public void SetAuto()
        {
            active = !active;
            if (AutoPlayController.Instance && AutoPlayController.Instance.holder)
            {
                AutoPlayController.Instance.SetHolder(active);
                Player.Instance.disallowInput = active;
            }
        }
    }
}