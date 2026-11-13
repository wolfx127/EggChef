using Cysharp.Threading.Tasks;
using Framework;
using Overall;
using Play.Hand;
using Play.Pot;
using SO.Play;
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
        [SerializeField] private float topCookPercent;
        [SerializeField] private float bottomCookPercent;
        [SerializeField]private bool isTopSide;//表示现在煎蛋是否正面朝上
        private Color rawColor;
        private Color perfectColor;
        private Color burnedColor;
        
        public bool isFindPot = false;
        private float findDis = 2f;
        private UniTaskLoopHandle findHandle;//表示方法（进程）执行的状态，开始、循环、暂停、继续

        public Side handSide = Side.NULL;

        public void InitFromEgg(Vector3 pos, FriedEggSO so)
        {
            collider = GetComponent<Collider>();
            rb = GetComponent<Rigidbody>();
            gameObject.SetActive(true);
            collider.isTrigger = false;
            rb.isKinematic = false;
            transform.position = pos;
            transform.rotation = new Quaternion(0, 0, 0, 1);
            rawColor = so.rawFriedEgg_Color;
            perfectColor = so.perfectFriedEgg_Color;
            burnedColor = so.burnedFriedEgg_Color;
            isTopSide = true;
            
            topFaceGO = transform.GetChild(0).gameObject;
            bottomFaceGO = transform.GetChild(1).gameObject;
            topCookPercent = 0;
            bottomCookPercent = 0;
            topFaceRenderer = topFaceGO.GetComponent<Renderer>();
            topFaceRenderer.material.color = rawColor;
            bottomFaceRenderer = bottomFaceGO.GetComponent<Renderer>();
            bottomFaceRenderer.material.color = rawColor;
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

            handSide = Side.NULL;
            collider.isTrigger = false;
            rb.isKinematic = true;
            transform.SetParent(pot.transform, false);
            transform.localPosition = Vector3.zero;
            isFrying = true;
            pot.GetComponent<PotData>().SetEgg(this);
            SnapToFace();
            
            //当前面都准备好后，再启动煎制循环
            fryHandle = UniTaskMgr.Repeat(0.4f, FryUpdate, this.GetCancellationTokenOnDestroy());
        }

        private void FryUpdate()
        {
            if (!isFrying)
            {
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
                        Color.Lerp(rawColor, perfectColor, bottomCookPercent/50f);
                }
                else
                {
                    bottomFaceRenderer.material.color = 
                        Color.Lerp(perfectColor, burnedColor, (bottomCookPercent-50f)/50f);
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
                        Color.Lerp(rawColor, perfectColor, topCookPercent/50f);
                }
                else
                {
                    topFaceRenderer.material.color = 
                        Color.Lerp(perfectColor, burnedColor, (topCookPercent-50f)/50f);
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
                pot.GetComponent<PotData>().perfectSpeed/3f, 
                Random.Range(-3f, 3f));
            
            isFrying = false;
            pot = null;
        }
        
        //摆正位置
        private void SnapToFace()
        {
            float angle = Vector3.Angle(transform.up, Vector3.up);
            Vector3 targetUp = Vector3.up;
            
            if (angle > 90f)
                targetUp = Vector3.down;

            Quaternion aligned = Quaternion.FromToRotation(transform.up, targetUp) * transform.rotation;

            if (rb && !rb.isKinematic)
                rb.MoveRotation(aligned);
            else
                transform.rotation = aligned;
        }

        public void FlipFace()
        {
            SnapToFace();
            if (Vector3.Dot(transform.up, Vector3.up) > 0f)
                isTopSide = true;
            else
                isTopSide = false;
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
