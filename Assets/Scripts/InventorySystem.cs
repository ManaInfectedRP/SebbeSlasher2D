using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

namespace Sebbe
{
    public class InventorySystem : MonoBehaviour
    {
        public static InventorySystem instance;
        // True when the inventory UI is open
        public bool inventoryOpen = false;
        
        [Header("UI References")]
        [SerializeField] private GameObject inventoryUI;
        [SerializeField] private Transform slotsHolder;
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private int slotCount = 42;

        [Header("Equipement Slots")]
        public GameObject weaponSlot;
        public GameObject helmetSlot;
        public GameObject amuletSlot;
        public GameObject armorSlot;
        public GameObject bootsSlot;
        public GameObject ringSlot;
        public GameObject keySlot;

        [Header("Instantiated Slots")]
        public List<GameObject> instantiatedSlots = new List<GameObject>();


        [Header("Inventory Items")]
        [SerializeField] private List<ItemSO> items = new List<ItemSO>();


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
        }

        private void Start()
        {
            InitializeInventoryUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                ToggleInventoryUI();
            }
        }

        public void ToggleInventoryUI()
        {
            if (inventoryUI != null)
            {
                bool newState = !inventoryUI.activeSelf;
                inventoryUI.SetActive(newState);
                inventoryOpen = newState;
            }
        }

        private void InitializeInventoryUI()
        {
            for (int i = 0; i < slotCount; i++)
            {
                GameObject slot = Instantiate(slotPrefab, slotsHolder);
                instantiatedSlots.Add(slot);
            }
            // Ensure inventoryOpen matches the initial UI state
            if (inventoryUI != null)
                inventoryOpen = inventoryUI.activeSelf;
        }
    }
}