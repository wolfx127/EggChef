using Cysharp.Threading.Tasks;
using Framework;
using Overall;
using Play.Hand;
using Play.Pot;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Play.Egg
{
    //控制煎蛋
    public class FriedEggData : MonoBehaviour
    {
        public Collider collider;
        public Rigidbody rb;
        
        public bool isFrying = false;
        private GameObject pot;
        private UniTaskLoopHandle fryHandle;
        private GameObject topFaceGO;
        private GameObject bottomFaceGO;
        private Renderer topFaceRenderer;
        private Renderer bottomFaceRenderer;
        private float topCookPercent;
        private float bottomCookPercent;
        private bool isTopSide;//表示现在煎蛋是否正面朝上
        
        public bool isFindPot = false;
        private float findDis = 2f;
        private UniTaskLoopHandle findHandle;//表示方法（进程）执行的状态，开始、循环、暂停、继续

        public Side handSide = Side.NULL;

        public void InitFromEgg(Vector3 pos)
        {
            collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            gameObject.SetActive(true);
            collider.isTrigger = false;
            rb.isKinematic = false;
            transform.position = pos;
            transform.rotation = new Quaternion(0, 0, 0, 1);
            
            topFaceGO = transform.GetChild(0).gameObject;
            bottomFaceGO = transform.GetChild(1).gameObject;
            topCookPercent = 0;
            bottomCookPercent = 0;
            topFaceRenderer = topFaceGO.GetComponent<Renderer>();
            topFaceRenderer.material.color = OverallData.rawFriedEgg_Color;
            bottomFaceRenderer = bottomFaceGO.GetComponent<Renderer>();
            bottomFaceRenderer.material.color = OverallData.rawFriedEgg_Color;
        }

        public void StartFry()
        {
            if (!pot)
            {
                isFrying = false;
                return;
            }

            //蛋已经进锅，不用再找锅了
            findHandle?.Stop();
            findHandle = null;
            fryHandle?.Stop();
            //启动煎制循环
            fryHandle = UniTaskMgr.Repeat(0.4f, FryUpdate, this.GetCancellationTokenOnDestroy());
            

            handSide = Side.NULL;
            collider.isTrigger = false;
            rb.isKinematic = true;
            transform.SetParent(pot.transform, false);
            transform.localPosition = Vector3.zero;
            isFrying = true;
            pot.GetComponent<PotData>().SetEgg(this);
            SnapToFace();
        }

        private void FryUpdate()
        {
            if (!isFrying)
            {
                fryHandle?.Stop();
                fryHandle = null;
                StopFry();
                return;
            }

            if (isTopSide)
            {
                if (bottomCookPercent >= 100 || bottomCookPercent<0)
                {
                    bottomCookPercent = 100;
                    return;
                }
                bottomCookPercent += OverallData.cookSpeed * 0.4f;
                if(bottomCookPercent <= 50)
                {
                    bottomFaceRenderer.material.color = 
                        Color.Lerp(OverallData.rawFriedEgg_Color, OverallData.perfectFriedEgg_Color, 
                            bottomCookPercent/50f);
                }
                else
                {
                    bottomFaceRenderer.material.color = 
                        Color.Lerp(OverallData.rawFriedEgg_Color, OverallData.perfectFriedEgg_Color, 
                            (bottomCookPercent-50f)/50f);
                }
            }
            else
            {
                if (topCookPercent >= 100 || topCookPercent < 0)
                {
                    topCookPercent = 100;
                    return;
                }

                topCookPercent += OverallData.cookSpeed * 0.4f;

                if (topCookPercent <= 50)
                {
                    topFaceRenderer.material.color = 
                        Color.Lerp(OverallData.rawFriedEgg_Color, OverallData.perfectFriedEgg_Color,
                        topCookPercent / 50f);
                }
                else
                {
                    topFaceRenderer.material.color = 
                        Color.Lerp(OverallData.rawFriedEgg_Color, OverallData.perfectFriedEgg_Color,
                        (topCookPercent - 50f) / 50f);
                }
            }
        }

        public void StopFry()
        {
            fryHandle?.Stop();
            fryHandle = null;
            
            transform.SetParent(null, true);
            transform.position = pot.transform.position + Vector3.up * 0.8f;
            collider.isTrigger = false;
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.linearVelocity = new Vector3(0, 
                pot.GetComponent<PotData>().perfectSpeed/1.5f, 
                Random.Range(-0.5f, 0.5f));
            
            isFrying = false;
            pot = null;
        }
        
        //摆正位置
        private void SnapToFace()
        {
            float angle = Vector3.Angle(transform.up, Vector3.up);
            Vector3 targetUp = Vector3.up;
            if (angle <= 90f)
            {
                isTopSide = true;
            }
            else
            {
                targetUp = Vector3.down;
                isTopSide = false;
            }

            Quaternion aligned = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;

            if (rb && !rb.isKinematic)
                rb.MoveRotation(aligned);
            else
                transform.rotation = aligned;
        }

        public void FlipFace()
        {
            isTopSide = !isTopSide;
            transform.Rotate(180f, 0f, 0f, Space.Self);
            SnapToFace();
        }

        public void EnterFromHand()
        {
            findHandle?.Stop();
            findHandle = UniTaskMgr.Repeat(0.1f, FindPot, this.GetCancellationTokenOnDestroy());
        }

        public void ExitFromHand()
        {
            handSide = Side.NULL;
            if(!isFrying)
            {
                collider.isTrigger = false;
                rb.useGravity = true;
                rb.isKinematic = false;
            }

            findHandle?.Stop();
            findHandle = null;

            Debug.Log("煎蛋已脱手");
        }

        private void FindPot()
        {
            if (Physics.Raycast(transform.position, Vector3.down, 
                    out RaycastHit hit, findDis, LayerMask.GetMask("FreezeItem"),
                    QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.gameObject &&
                    hit.collider.gameObject.CompareTag("Pot"))
                {
                    pot = hit.collider.gameObject;
                    isFindPot = true;
                    Debug.Log("找到锅了");
                }
            }
            else
            {
                isFindPot = false;
                pot = null;
            }
        }
    }
}
