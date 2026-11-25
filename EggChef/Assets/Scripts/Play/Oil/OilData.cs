using System;
using UnityEngine;
using Framework;
using Overall;
using Play.Hand;
using Play.Pot;
using SO.Play;

namespace Play.Oil
{
    public class OilData : MonoBehaviour
    {
        private float speed;
        
        public Side handSide = Side.NULL;
        private Vector3 originPos;
        private Vector3 originRot;

        private UniTaskLoopHandle FindPotHandle;
        private PotData pot;

        public bool isPouring = false;
        private UniTaskLoopHandle pourHandle;

        public Collider collider;
        
        public void Init(OilSO so)
        {
            Debug.Log("初始化：OilData构造");
            originPos = transform.position;
            collider = GetComponent<Collider>();
            speed = so.speed;
        }

        public void OnCatchEnter(Side side)
        {
            handSide = side;
            FindPotHandle?.Stop();
            FindPotHandle = UniTaskMgr.RepeatEveryFrame(FindPot);
            originPos = transform.position;
            originRot = transform.rotation.eulerAngles;
            collider.isTrigger = true;
            Debug.Log("被手抓住了");
        }

        public void OnCatchExit()
        {
            FindPotHandle?.Stop();
            handSide = Side.NULL;
            transform.position = originPos;
            transform.rotation = Quaternion.Euler(originRot);
            collider.isTrigger = false;
        }

        private void FindPot()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f) &&
                hit.collider.TryGetComponent<PotData>(out PotData potData))
            {
                pot = potData;
                Debug.Log("找到锅了");
            }
            else
            {
                pot = null;
            }
        }

        public void EnterPour(float dt)
        {
            isPouring = !isPouring;
            if (pot && OverallData.oilCount > 0 && isPouring)
            {
                transform.rotation = Quaternion.Euler(Vector3.down);
                pourHandle?.Stop();
                pourHandle = UniTaskMgr.RepeatEveryFrame(Pour);
                Debug.Log("正在倒油");
            }
            else
            {
                OverallData.oilCount = 0;
                CancelPour();
            }
        }

        private void Pour()
        {
            if (!pot) return;
            pot.AddOil(speed * Time.deltaTime);
            OverallData.oilCount -= speed * Time.deltaTime;
        }

        public void CancelPour()
        {
            pot = null;
            isPouring = false;
            transform.rotation = Quaternion.Euler(originRot);
        }


        public bool CanPour()
        {
            pourHandle?.Stop();
            pourHandle = null;
            return pot && OverallData.oilCount > 0;
        }

        public void OnDestroy()
        {
            FindPotHandle?.Stop();
            FindPotHandle = null;
            pourHandle?.Stop();
            pourHandle = null;
        }
    }
}