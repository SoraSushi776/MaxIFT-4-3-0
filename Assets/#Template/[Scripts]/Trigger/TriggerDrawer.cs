#if UNITY_EDITOR
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [RequireComponent(typeof(BoxCollider))]
    public class TriggerDrawer : MonoBehaviour
    {
        public Color color = new (0, 1, 0, 0.6f);
        private void OnDrawGizmos()
        {
            var col = GetComponent<BoxCollider>();
            Gizmos.matrix = this.transform.localToWorldMatrix;
            Gizmos.color = color;
            Gizmos.DrawCube(col.center, col.size);
        }
    }
}
#endif