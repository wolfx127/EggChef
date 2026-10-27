using SO.Play;
using UnityEngine;

namespace Play.Egg
{
    public class EggCase : MonoBehaviour
    {
        private EggSO eggSO;
        private GameObject eggGO;
        private EggData egg;

        public void Init(GameObject eggGO, EggSO eggSO)
        {
            Debug.Log("初始化：EggCase");
            this.eggGO = eggGO;
            this.eggSO = eggSO;
            
            eggGO.SetActive(false);
            egg = new EggData(eggSO.perfectSpeed, eggSO.badSpeed);
        }
    }
}
