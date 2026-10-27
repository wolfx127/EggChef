using System;
using Play.Egg;
using Play.Hand;
using UnityEngine;

namespace Play
{
    //负责需要调用monoAPI的逻辑,不包括输入输出; 包括状态机tick,动画
    public class PlayMonoController : MonoBehaviour
    {
        private PlayCamera camera;
        private GameObject cameraGO;
        private HandData leftHand;
        private HandData rightHand;

        private void Update()
        {
            leftHand.Tick(Time.deltaTime);
            rightHand.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            rightHand.Move(Time.fixedDeltaTime);
            leftHand.Move(Time.fixedDeltaTime);
        }

        private void LateUpdate()
        {
            
        }

        public void Init(GameObject cameraGO, PlayCamera camera, HandData leftHand, HandData rightHand)
        {
            Debug.Log("初始化：PlayMonoController");
            this.cameraGO = cameraGO;
            this.camera = camera;
            this.leftHand = leftHand;
            this.rightHand = rightHand;
        }
    }
}
