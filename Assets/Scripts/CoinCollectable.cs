using UnityEngine;

namespace Sebbe
{
    public class CoinCollectable : MonoBehaviour
    {
        [HideInInspector] public CoinManager coinManager;

        [Header("Animation Settings")]
        [SerializeField] float bounceHeight = 0.25f;
        [SerializeField] float rotationSpeed = 50f;
        [SerializeField] private Vector3 initialPosition;

        public int value = 1;

        private void Start()
        {
            if (initialPosition == Vector3.zero)
                initialPosition = transform.position;

            coinManager = CoinManager.instance;
        }

        private void Update()
        {
            // Bounce effect
            float newY = Mathf.Sin(Time.time * 2f) * bounceHeight;
            transform.position = new Vector3(transform.position.x, newY + initialPosition.y, transform.position.z);

            // Rotate effect
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                coinManager.ChangeCoins(value);
                // Here you can add code to update the player's coin count or score
                Destroy(gameObject);
            }
        }
        
    }
} 