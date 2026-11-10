using System;
using System.Collections.Generic;
using Play.Egg;
using Play.Hand;
using Play.Pot;
using Play.UI;
using UnityEngine;

namespace Play
{
    //负责需要调用monoAPI的逻辑,不包括输入输出; 包括状态机tick,动画
    //主要是使用update系列函数
    public class PlayMonoController : MonoBehaviour
    {
        private PlayCamera camera;
        private GameObject cameraGO;
        private HandData leftHand;
        private HandData rightHand;
        private List<PotData> potList = new List<PotData>();

        private void Start()
        {
            foreach (var pot in potList)
            {
                pot.StartTick();
            }
        }

        private void Update()
        {
            leftHand.Tick(Time.deltaTime);
            rightHand.Tick(Time.deltaTime);
            foreach (var pot in potList)
            {
                pot.Tick(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            
        }

        private void LateUpdate()
        {
            
        }

        public void Init(GameObject cameraGO, PlayCamera camera, HandData leftHand, HandData rightHand, List<GameObject> potList)
        {
            Debug.Log("初始化：PlayMonoController");
            this.cameraGO = cameraGO;
            this.camera = camera;
            this.leftHand = leftHand;
            this.rightHand = rightHand;
            foreach (var pot in potList)
                this.potList.Add(pot.GetComponent<PotData>());
        }
    }
}
