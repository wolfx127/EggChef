using UnityEngine;

namespace SO.Play
{
    [CreateAssetMenu(fileName = "BowSO", menuName = "PlaySO/BowSO")]
    public class BowSO : ScriptableObject
    {
        public float drawTrackBallGapDis;
        public float minDis_horizontal;
        public float maxDis_horizontal;
        public float minDis_vertical;
        public float maxDis_vertical;
        
        public Mesh trackBallMesh;
        public Material trackBallMat;
    }
}
