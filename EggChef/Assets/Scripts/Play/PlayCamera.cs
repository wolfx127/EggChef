using SO.Play;
using UnityEngine;

namespace Play
{
    public class PlayCamera
    {
        private PlayCameraSO cameraSO;

        public float speed_upNdown => cameraSO.speed_upNdown;
        public float speed_leftNright => cameraSO.speed_leftNright;
        public float leftRange => cameraSO.leftRange;
        public float rightRange => cameraSO.rightRange;
        public float UpRange => cameraSO.UpRange;
        public float DownRange => cameraSO.DownRange;

        public float beginningX = 0;
        public float beginningY = 0;
        public float curX = 0;
        public float curY = 0;
        
        public PlayCamera(GameObject cameraGO, PlayCameraSO cameraSO)
        {
            cameraGO.transform.rotation = Quaternion.Euler(beginningY, beginningX, 0f);
            this.cameraSO = cameraSO;
            beginningX = cameraSO.beginningX;
            beginningY = cameraSO.beginningY;
        }
        
        public void MoveCamera(GameObject cameraGO, PlayInputAction inputAction, float dt)
        {
            Vector2 v = inputAction.map.Rotation.ReadValue<Vector2>();

            curX = Mathf.Clamp(curX + v.x * speed_leftNright * dt, leftRange, rightRange);
            curY = Mathf.Clamp(curY - v.y * speed_upNdown * dt, UpRange, DownRange); 

            cameraGO.transform.rotation = Quaternion.Euler(beginningY + curY, beginningX + curX, 0f);
        }
    }
}
