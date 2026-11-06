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
            egg = eggGO.GetComponent<EggData>();
            egg.Init(eggSO, this);
        }

        public EggData GetEgg()
        {
            EggData newEgg = null;
            if (eggCount > 0)
            {
                GameObject newEggGO = Instantiate(eggGO);
                newEggGO.transform.SetParent(eggShellGO.transform);
                newEggGO.transform.position = eggShellGO.transform.position;
                newEggGO.GetComponent<Collider>().isTrigger = true;
                newEggGO.GetComponent<Rigidbody>().useGravity = false;
                newEggGO.SetActive(true);
                newEgg = newEggGO.GetComponent<EggData>();
                newEgg.Init(eggSO, this);

                eggCount--;
                Debug.Log("成功给出一个新鸡蛋");
            }
            else Debug.Log($"鸡蛋数量不够{'\n'}可能的原因：①鸡蛋给玩光了；②鸡蛋数没初始化，默认值是0，需要重回start场景启动一下");
            return newEgg;
        }

        public void ReturnEgg(EggData egg)
        {
            Destroy(egg.gameObject);
            eggCount++;
            Debug.Log("鸡蛋已安全放回");
        }
    }
}
