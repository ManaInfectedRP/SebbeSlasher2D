using UnityEngine;
using TMPro;

namespace Sebbe
{
    public class CoinManager : MonoBehaviour
    {
        public static CoinManager instance;
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    
        public int totalCoins = 0;
        public TMP_Text coinText;

        void Start()
        {
            if (coinText != null)
                coinText.text = "Coins: " + totalCoins;
        }

        public void ChangeCoins(int amount)
        {
            totalCoins += amount;
            if (coinText != null)
                coinText.text = "Coins: " + totalCoins;

            // Use the centralized achievement API to add progress and let it handle unlocking/UI
            if (WorldAchivementManager.instance != null)
            {
                WorldAchivementManager.instance.AddProgress(1, amount);
            }
        }
    }
}