using UnityEngine;

namespace SO.Play
{
    [CreateAssetMenu(fileName = "PotSO", menuName = "PlaySO/PotSO")]
    public class PotSO : ScriptableObject
    {
        public float perfectSpeed;
        public float badSpeed;
        public float maxHeight;
        public float minOil;
        public float maxOil;
        public float beginningFlipSpeed;
        public float flipGravity;
        public float flipAngleSpeed;
    }
}
