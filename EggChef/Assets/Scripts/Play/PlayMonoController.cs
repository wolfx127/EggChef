using System;
using UnityEngine;

namespace Play
{
    //负责需要调用monoAPI的逻辑,不包括输入输出; 包括状态机tick,动画
    public class PlayMonoController : MonoBehaviour
    {
        private PlayCamera camera;
        private GameObject cameraGO;

        private void Update()
        {
            
        }

        private void LateUpdate()
        {
            
        }

        public void Init(GameObject cameraGO, PlayCamera camera)
        {
            this.cameraGO = cameraGO;
            this.camera = camera;
        }
    }
}
