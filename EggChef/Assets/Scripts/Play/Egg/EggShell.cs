using Framework;
using Overall;
using SO.Play;
using UnityEngine;
using Object = System.Object;

namespace Play.Egg
{
    //游戏中交付鸡蛋的工厂
    //mono是因为要射线击中蛋筐，方便击中后直接返回脚本类
    public class EggShell : MonoBehaviour
    {
        private EggSO eggSO;
        private GameObject eggGO;
        private EggData egg;
        private GameObject eggShellGO;

        private int eggCount {get =>OverallData.eggCount; set =>OverallData.eggCount = value; }

        public void Init(GameObject eggGO, EggSO eggSO, GameObject eggShellGO)
        {
            Debug.Log("初始化：EggCase构造");
            this.eggGO = eggGO;
            this.eggSO = eggSO;
            this.eggShellGO = eggShellGO;
            
            eggGO.SetActive(false);
            egg = new EggData(eggSO.perfectSpeed, eggSO.badSpeed);
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

                eggCount--;
            }
            return newEgg;
        }
    }
}
