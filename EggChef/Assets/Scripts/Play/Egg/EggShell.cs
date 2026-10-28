using SO.Play;
using UnityEngine;

namespace Play.Egg
{
    public class EggShell
    {
        private EggSO eggSO;
        private GameObject eggGO;
        private EggData egg;

        public EggShell(GameObject eggGO, EggSO eggSO)
        {
            Debug.Log("初始化：EggCase构造");
            this.eggGO = eggGO;
            this.eggSO = eggSO;
            
            eggGO.SetActive(false);
            egg = new EggData(eggSO.perfectSpeed, eggSO.badSpeed);
        }
    }
}
