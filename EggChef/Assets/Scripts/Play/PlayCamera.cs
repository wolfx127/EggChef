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
        
        public PlayCamera(PlayCameraSO cameraSO)
        {
            this.cameraSO = cameraSO;
            beginningX = cameraSO.beginningX;
            beginningY = cameraSO.beginningY;
        }
    }
}
