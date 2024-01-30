using DancingLineFanmade.Level;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace DancingLineFanmade.Trigger
{
    [Serializable]
    public struct SingleActive
    {
        public GameObject target;
        public bool active;
        public bool dontRevive;

        public SingleActive(GameObject target, bool active, bool dontRevive)
        {
            this.target = target;
            this.active = active;
            this.dontRevive = dontRevive;
        }

        public void SetActive()
        {
            target.SetActive(active);
        }
    }

    [DisallowMultipleComponent]
    public class SetActive : MonoBehaviour
    {
        [SerializeField] internal bool activeOnAwake = false;
        [SerializeField, TableList] internal List<SingleActive> actives = new List<SingleActive>();

        private List<SingleActive> revives = new List<SingleActive>();
        internal int index;

        private void Start()
        {
            if (activeOnAwake) foreach (SingleActive s in actives) s.SetActive();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && !activeOnAwake)
            {
                index = Player.Instance.Checkpoints.Count;
                foreach (SingleActive s in actives) s.SetActive();
            }
        }

        internal void AddRevives()
        {
            for (int a = 0; a < actives.Count; a++) revives.Add(new SingleActive(actives[a].target, actives[a].target.activeSelf, actives[a].dontRevive));
        }

        internal void Revive()
        {
            LevelManager.CompareCheckpointIndex(index, () =>
            {
                foreach (SingleActive s in revives) if (!s.dontRevive) s.SetActive();
            });
        }
    }
}