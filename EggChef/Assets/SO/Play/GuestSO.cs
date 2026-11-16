using UnityEngine;

namespace SO.Play
{
    [CreateAssetMenu(fileName = "GuestSO", menuName = "PlaySO/GuestSO")]
    public class GuestSO : ScriptableObject
    {
        public float sightDis;
        public float walkSpeed;
    }
}
