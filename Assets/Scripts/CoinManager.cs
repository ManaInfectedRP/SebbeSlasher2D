using UnityEngine;
using TMPro;
using System;

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

        // Event invoked when coin total changes. Parameter = new totalCoins
        public Action<int> OnCoinsChanged;

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

            // Invoke change event
            OnCoinsChanged?.Invoke(totalCoins);

            // Use the centralized achievement API to add progress and let it handle unlocking/UI
            if (WorldAchivementManager.instance != null)
            {
                WorldAchivementManager.instance.AddProgress(1, amount);
            }
        }
    }
}