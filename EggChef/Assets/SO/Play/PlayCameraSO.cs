using UnityEngine;

namespace SO.Play
{
    [CreateAssetMenu(fileName = "PlayCameraSO", menuName = "PlaySO/PlayCameraSO")]
    public class PlayCameraSO : ScriptableObject
    {
        public float speed_upNdown;
        public float speed_leftNright;
        public float leftRange;
        public float rightRange;
        public float UpRange;
        public float DownRange;
        public float beginningX;
        public float beginningY;
    }
}
