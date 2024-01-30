using DancingLineFanmade.Level;
using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [RequireComponent(typeof(Collider))]
    public class SetImageColor : MonoBehaviour
    {
        [SerializeField, TableList] private List<SingleImage> images = new List<SingleImage>();
        [SerializeField] private float duration = 2f;
        [SerializeField] private Ease ease = Ease.InOutSine;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) foreach (SingleImage s in images) s.SetColor(duration, ease);
        }
    }
}