using Framework;
using Overall;
using SO.Play;
using UnityEngine;
using Object = System.Object;

namespace Play.Egg
{
    //游戏中交付鸡蛋的工厂
    public class EggShell
    {
        private EggSO eggSO;
        private GameObject eggGO;
        private EggData egg;
        private GameObject eggShellGO;

        private int eggCount => OverallData.eggCount;

        public EggShell(GameObject eggGO, EggSO eggSO, GameObject eggShellGO)
        {
            Debug.Log("初始化：EggCase构造");
            this.eggGO = eggGO;
            this.eggSO = eggSO;
            this.eggShellGO = eggShellGO;
            
            eggGO.SetActive(false);
            egg = new EggData(eggSO.perfectSpeed, eggSO.badSpeed);
            EventBus.Subscribe<ReturnEggEvent>(ReturnEgg);
        }

        private void ReturnEgg(ReturnEggEvent evt)
        {
            evt.eggData = GetEgg();
        }

        public EggData GetEgg()
        {
            EggData newEgg = null;
            if (eggCount > 0)
            {
                newEgg = new EggData(egg.perfectSpeed, egg.perfectSpeed);
                GameObject newEggGO = UnityEngine.Object.Instantiate(eggGO);
                newEggGO.transform.SetParent(eggShellGO.transform);
                newEggGO.transform.position = eggShellGO.transform.position;
                newEggGO.GetComponent<Collider>().isTrigger = true;
                newEggGO.GetComponent<Rigidbody>().useGravity = false;
                newEggGO.SetActive(true);
                newEgg.eggGO = newEggGO;
            }
            return newEgg;
        }
    }
}
