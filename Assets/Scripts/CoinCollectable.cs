using UnityEngine;

namespace Sebbe
{
    public class CoinCollectable : MonoBehaviour
    {
        [HideInInspector] public CoinManager coinManager;

        public int value = 1;

        private void Start()
        {
            coinManager = CoinManager.instance;
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