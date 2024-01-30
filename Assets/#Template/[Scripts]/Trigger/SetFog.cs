using DG.Tweening;
using DancingLineFanmade.Level;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [DisallowMultipleComponent, RequireComponent(typeof(Collider))]
    public class SetFog : MonoBehaviour
    {
        [SerializeField] private FogSettings fog;
        [SerializeField] private float duration = 2f;
        [SerializeField] private Ease ease = Ease.Linear;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) fog.SetFog(Player.Instance.sceneCamera, duration, ease);
        }
    }
}