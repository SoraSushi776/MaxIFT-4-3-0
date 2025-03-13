using UnityEngine;

namespace DancingLineFanmade.UI{
    public class CrownParticleSign : MonoBehaviour{
        public ParticleSystem particle;
        private void Start(){
            if(particle == null){
                particle = GetComponentInChildren<ParticleSystem>();
            }
        }
    }
}
