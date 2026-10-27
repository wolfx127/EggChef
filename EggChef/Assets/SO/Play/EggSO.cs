using UnityEngine;

namespace SO.Play
{
    [CreateAssetMenu(fileName = "EggSO", menuName = "PlaySO/EggSO")]
    public class EggSO : ScriptableObject
    {
        public float perfectSpeed;
        public float badSpeed;
    }
}
