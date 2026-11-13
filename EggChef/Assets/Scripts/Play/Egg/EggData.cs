using System;
using Play.Hand;
using Play.Pot;
using SO.Play;
using UnityEngine;

namespace Play.Egg
{
    public enum EggState
    {
        NULL,
        normal,
        perfect,
        bad
    }
    //控制鸡蛋
    public class EggData : MonoBehaviour
    {
        public float perfectSpeed;
        public float badSpeed;
        public bool isPerfect;
        public bool isBad;
        private EggShell eggShell;
        public Rigidbody rb;
        public Collider collider;
        private float friction = 0;
        private FriedEggSO friedEggSO;
        
        public Side handSide = Side.NULL;
        
        private GameObject friedEggGO = null;
        public bool isPlayingAnim = false;
        public float animTime = 0;
        public float animTimeCounter = 0;

        public void Init(EggSO so, Side handSide, EggShell eggShell, GameObject friedEggGO, FriedEggSO friedEggSO)
        {
            Debug.Log("初始化：EggData构造");
            perfectSpeed = so.perfectSpeed;
            badSpeed = so.badSpeed;
            friction = so.friction;
            animTime = so.animTime;
            animTimeCounter = so.animTime;
            this.handSide = handSide;
            this.eggShell = eggShell;
            this.friedEggGO = friedEggGO;
            this.friedEggSO = friedEggSO;
            
            isPerfect = false;
            isBad = false;
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();
        }

        public void FixedUpdate()
        {
            Frict(Time.fixedDeltaTime);
        }

        public void TurnPerfectEgg()
        {
            if (isBad || isPerfect) return;
            isPerfect = true;
            isBad = false;
            
            //TODO:换模型
            
            Debug.Log("敲得完美");
        }

        public void TurnBadEgg()
        {
            if (isBad) return;
            isBad = true;
            isPerfect = false;

            //TODO:换模型

            Debug.Log("敲得烂");
        }

        public void ExitFromHand()
        {
            handSide = Side.NULL;
            if (isPerfect)
            {
                TurnBadEgg();
            }
            if (isBad)
            {
                rb.useGravity = true;
                collider.isTrigger = false;
            }
            else
            {
                eggShell.ReturnEgg(this);
            }
            Debug.Log("鸡蛋已脱手");
        }
        
        //摩擦，使鸡蛋匀减速运动直至停下，不至于一直滑行
        private void Frict(float dt)
        {
            Vector2 v = new Vector2(rb.linearVelocity.x, rb.linearVelocity.z);
            //缓存模长，避免重复计算
            float mag = v.magnitude;
            //下面除法，分母不能为零
            if (mag == 0) return;
            //摩擦力按 边长/模长 分解
            Vector2 newV = new Vector2(v.x*(1 - friction*dt/mag), v.y*(1 - friction*dt/mag));
            //符号不同（一正一负），说明方向反了
            if (newV.x * v.x <= 0) newV.x=0;
            if (newV.y * v.y <= 0) newV.y=0;
            rb.linearVelocity = new Vector3(newV.x, rb.linearVelocity.y, newV.y);
        }

        /// <summary>
        /// 生成煎蛋
        /// </summary>
        public void PutEgg()
        {
            if (!isPerfect) return;
            
            GameObject go = Instantiate(friedEggGO, eggShell.gameObject.transform, true);
            go.GetComponent<FriedEggData>().InitFromEgg(transform.position, friedEggSO);
            TurnBadEgg();
            animTimeCounter = 0;
        }

        public void PutAnim(float dt)
        {
            if (animTimeCounter >= animTime)
            {
                isPlayingAnim = false;
                return;
            }

            animTimeCounter += dt;
            isPlayingAnim = true;
        }
    }
}
