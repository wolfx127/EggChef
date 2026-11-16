using UnityEngine;

namespace SO.Play
{
    [CreateAssetMenu(fileName = "GuestCreatorSO", menuName = "PlaySO/GuestCreatorSO")]
    public class GuestCreatorSO : ScriptableObject
    {
        public float createGapTime = 0;
        public float roadNodeLinkDis;
    }
}
