using UnityEngine;

namespace Play.Decoration
{
    public class RoadNode : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.6f);
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }
}
