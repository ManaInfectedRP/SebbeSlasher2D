using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using TMPro;

namespace Sebbe
    {
    public class Shop : MonoBehaviour
    {
        public static Shop instance;
        
        [System.Serializable]
        public class ShopItem
        {
            public ItemSO item;
            public int price;
        }

        [Header("Shop Inventory")]
        [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

        [Header("UI References")]
        [SerializeField] private GameObject shopUI;
        [SerializeField] private Transform shopItemsHolder;
        [SerializeField] private GameObject shopItemPrefab;
        [SerializeField] private GameObject tradeRecipePrefab;
        [Header("Trade Recipes")]
        [SerializeField] private List<TradeRecipe> tradeRecipes = new List<TradeRecipe>();

        public enum ShopMode { Buy, Trade }
        private ShopMode currentMode = ShopMode.Buy;

        [Header("Interaction")]
        public float interactRadius = 2f;
        public KeyCode interactKey = KeyCode.E;
        [Tooltip("Optional prefab for the 'Press [E] to open shop' prompt. If unset a simple TextMeshPro object is created at runtime.")]
        public GameObject promptPrefab;
        [Tooltip("Local offset for the prompt when instantiated as a child of the Shop object.")]
        public Vector3 promptLocalOffset = new Vector3(0f, 0f, 0f);

        private Transform playerT;
        private GameObject promptInstance;
        private bool shopOpen = false;
        private AudioSource audioSource;

        [Header("Audio (optional)")]
        public AudioClip purchaseClip;
        public AudioClip failClip;

        private void Awake()
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

            // Ensure an AudioSource for feedback if clips are assigned
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }
        }

        private void Update()
        {
            if (Player.instance != null) playerT = Player.instance.transform;
            if (playerT == null) return;

            float dist = Vector2.Distance(transform.position, playerT.position);
            if (dist <= interactRadius)
            {
                if (!shopOpen)
                {
                    EnsurePromptVisible();
                    if (Input.GetKeyDown(interactKey))
                    {
                        OpenShop();
                    }
                }
                else
                {
                    // shop is open, allow closing with same key
                    if (Input.GetKeyDown(interactKey) || Input.GetKeyDown(KeyCode.Escape))
                    {
                        CloseShop();
                    }
                }
            }
            else
            {
                EnsurePromptHidden();
                if (shopOpen) CloseShop();
            }
        }

        public void OpenShop()
        {
            if (shopUI != null)
            {
                shopUI.SetActive(true);
                shopOpen = true;
                EnsurePromptHidden();
                PopulateShopUI();
            }
        }

        public void CloseShop()
        {
            if (shopUI != null)
            {
                shopUI.SetActive(false);
                shopOpen = false;
            }
        }

        private void PopulateShopUI()
        {
            // Basic implementation: designer may populate UI manually; keep this minimal.
            // If shopItemsHolder is assigned, clear children so designer can instantiate item prefabs at runtime.
            if (shopItemsHolder == null) return;

            for (int i = shopItemsHolder.childCount - 1; i >= 0; --i)
            {
                Destroy(shopItemsHolder.GetChild(i).gameObject);
            }

            // Instantiate entries depending on current mode
            if (currentMode == ShopMode.Buy)
            {
                if (shopItemPrefab == null) return;
                for (int s = 0; s < shopItems.Count; ++s)
                {
                    var entryGO = Instantiate(shopItemPrefab, shopItemsHolder);
                    var ui = entryGO.GetComponent<ShopItemUI>();
                    if (ui != null)
                    {
                        ui.Setup(shopItems[s], s);
                    }
                }
            }
            else // Trade mode
            {
                if (tradeRecipePrefab == null) return;
                for (int t = 0; t < tradeRecipes.Count; ++t)
                {
                    var entryGO = Instantiate(tradeRecipePrefab, shopItemsHolder);
                    var ui = entryGO.GetComponent<TradeRecipeUI>();
                    if (ui != null)
                    {
                        ui.Setup(tradeRecipes[t], t);
                    }
                }
            }
        }

        // Switch the shop between Buy and Trade modes and refresh the UI
        public void SetModeToBuy()
        {
            currentMode = ShopMode.Buy;
            if (shopOpen) PopulateShopUI();
        }

        public void SetModeToTrade()
        {
            currentMode = ShopMode.Trade;
            if (shopOpen) PopulateShopUI();
        }

        [System.Serializable]
        public class TradeRecipe
        {
            // Items required to perform the trade. Duplicates mean multiple of same item required.
            public List<ItemSO> requiredItems = new List<ItemSO>();
            // Resulting item
            public ItemSO resultItem;
        }

        // Attempt to perform a trade recipe at index in tradeRecipes. Returns true if successful.
        public bool AttemptTrade(int index)
        {
            if (index < 0 || index >= tradeRecipes.Count)
            {
                Debug.LogWarning("Shop: AttemptTrade called with invalid index.");
                return false;
            }

            var r = tradeRecipes[index];
            if (r == null || r.resultItem == null || r.requiredItems == null)
            {
                Debug.LogWarning("Shop: AttemptTrade - invalid recipe.");
                return false;
            }

            if (Player.instance == null || Player.instance.inventory == null)
            {
                Debug.LogWarning("Shop: Player or inventory not found.");
                return false;
            }

            // Build list of required item IDs (allow duplicates)
            var requiredIDs = new System.Collections.Generic.List<int>();
            foreach (var it in r.requiredItems)
            {
                if (it != null) requiredIDs.Add(it.itemID);
            }

            if (!Player.instance.inventory.HasItems(requiredIDs))
            {
                // Feedback
                if (FloatingTextManager.instance != null)
                    FloatingTextManager.instance.Spawn("Missing ingredients", Player.instance.transform.position + Vector3.up * 1.2f, Color.red, false, 1.2f);
                if (audioSource != null && failClip != null) audioSource.PlayOneShot(failClip);
                return false;
            }

            // Remove required items from player inventory
            Player.instance.inventory.RemoveItems(requiredIDs);

            // Give result item
            Player.instance.inventory.AddItem(r.resultItem.itemID);

            // Feedback
            if (FloatingTextManager.instance != null)
                FloatingTextManager.instance.Spawn($"Received {r.resultItem.itemName}", Player.instance.transform.position + Vector3.up * 1f, Color.green, false, 1.2f);
            if (audioSource != null && purchaseClip != null) audioSource.PlayOneShot(purchaseClip);

            Debug.Log($"Trade succeeded: Received {r.resultItem.itemName}");
            return true;
        }

        // Attempt to buy the shop item at index. Returns true if purchase succeeded.
        public bool BuyItem(int index)
        {
            if (index < 0 || index >= shopItems.Count)
            {
                Debug.LogWarning("Shop: BuyItem called with invalid index.");
                return false;
            }

            var s = shopItems[index];
            if (s == null || s.item == null)
            {
                Debug.LogWarning("Shop: BuyItem - item is null.");
                return false;
            }

            if (CoinManager.instance == null)
            {
                Debug.LogWarning("Shop: CoinManager not found. Can't complete purchase.");
                return false;
            }

            if (CoinManager.instance.totalCoins < s.price)
            {
                Debug.Log("Not enough coins to buy " + s.item.itemName);
                // Feedback: floating text and optional sound
                if (FloatingTextManager.instance != null)
                    FloatingTextManager.instance.Spawn("Not enough coins", Player.instance.transform.position + Vector3.up * 1.2f, Color.red, false, 1.2f);
                if (audioSource != null && failClip != null) audioSource.PlayOneShot(failClip);
                return false;
            }

            // Deduct coins
            CoinManager.instance.ChangeCoins(-s.price);

            // Add item to player inventory
            if (Player.instance != null && Player.instance.inventory != null)
            {
                Player.instance.inventory.AddItem(s.item.itemID);
            }

            // Feedback: floating text and optional sound
            if (FloatingTextManager.instance != null)
            {
                FloatingTextManager.instance.Spawn($"Bought {s.item.itemName}", Player.instance.transform.position + Vector3.up * 1.2f, Color.green, false, 1.2f);
            }
            if (audioSource != null && purchaseClip != null) audioSource.PlayOneShot(purchaseClip);

            Debug.Log("Purchased " + s.item.itemName + " for " + s.price + " coins.");
            return true;
        }

        private void EnsurePromptVisible()
        {
            if (promptInstance != null) return;

            if (promptPrefab != null)
            {
                promptInstance = Instantiate(promptPrefab, transform);
                promptInstance.transform.localPosition = promptLocalOffset;
                promptInstance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // create a simple TextMeshPro floating label
                promptInstance = new GameObject("ShopPrompt", typeof(TextMeshPro));
                var tmp = promptInstance.GetComponent<TextMeshPro>();
                tmp.text = $"Press [{interactKey}] to open shop";
                tmp.fontSize = 3f;
                tmp.color = Color.yellow;
                var rend = tmp.GetComponent<Renderer>();
                if (rend != null) rend.sortingOrder = 99;
                promptInstance.transform.SetParent(transform, false);
                promptInstance.transform.localPosition = promptLocalOffset;
                promptInstance.transform.localRotation = Quaternion.identity;
            }

            var cg = promptInstance.GetComponent<CanvasGroup>();
            if (cg == null) cg = promptInstance.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
        }

        private void EnsurePromptHidden()
        {
            if (promptInstance == null) return;
            Destroy(promptInstance);
            promptInstance = null;
        }
        
    }
}