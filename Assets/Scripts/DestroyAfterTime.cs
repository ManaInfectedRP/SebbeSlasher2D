using UnityEngine;

namespace Sebbe
{
    public class DestroyAfterTime : MonoBehaviour
    {
        [SerializeField] private float lifetime = 1f;

        private void Start()
        {
            Destroy(gameObject, lifetime);
        }
    }
}
