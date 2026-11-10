using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Play.Hand;
using Play.Pot;
using UnityEngine;

namespace Play.Egg
{
    //控制煎蛋
    public class FriedEggData : MonoBehaviour
    {
        public Collider collider;
        public Rigidbody rb;
        
        public bool isFrying = false;
        private GameObject pot;
        public bool isFindPot = false;
        private float findDis = 2f;
        private float findTimeScale = 0.1f;
        private CancellationTokenSource cts;

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
        }

        public void StartFry()
        {
            if (!pot)
            {
                isFrying = false;
                return;
            }

            collider.isTrigger = false;
            rb.isKinematic = true;
            transform.SetParent(pot.transform, false);
            transform.localPosition = Vector3.zero;
            isFrying = true;
            pot.GetComponent<PotData>().SetEgg(this);
        }

        public void FryUpdate(float dt)
        {
            if (!isFrying) return;
        }

        public void StopFry()
        {
            
        }

        public void EnterFromHand()
        {
            cts = new CancellationTokenSource();
            FindPotUpdate(cts.Token).Forget();
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
            cts?.Cancel();
            cts?.Dispose();
            cts = null;
            Debug.Log("煎蛋已脱手");
        }

        private async UniTaskVoid FindPotUpdate(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                FindPot();
                await UniTask.Delay(TimeSpan.FromSeconds(findTimeScale), cancellationToken: token);
            }
        }

        private void FindPot()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, findDis))
            {
                if (hit.collider.gameObject &&
                    hit.collider.gameObject.layer == LayerMask.NameToLayer("FreezeItem") &&
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
