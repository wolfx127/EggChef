using SO.Play;
using UnityEngine;

namespace Play.Egg
{
    public class EggCase : MonoBehaviour
    {
        private EggSO eggSO;
        private GameObject eggGO;
        private Egg egg;

        public void Init(GameObject eggGO, EggSO eggSO)
        {
            this.eggGO = eggGO;
            this.eggSO = eggSO;
            
            eggGO.SetActive(false);
            egg.perfectSpeed = eggSO.perfectSpeed;
            egg = new Egg(eggSO.perfectSpeed, eggSO.badSpeed);
        }
    }
}
