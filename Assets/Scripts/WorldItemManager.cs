using System.Collections.Generic;
using UnityEngine;

namespace Sebbe
{
    public class WorldItemManager : MonoBehaviour
    {
        public static WorldItemManager instance;

        [SerializeField] private List<ItemSO> allItems = new List<ItemSO>();

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
            // Generate ID for each item if not set
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i].itemID == 0)
                {
                    allItems[i].itemID = i + 1; // IDs start from 1
                    Debug.Log($"Assigned ID {allItems[i].itemID} to item {allItems[i].itemName}");
                }
            }
        }

        public ItemSO GetItemByID(int id)
        {
            return allItems.Find(item => item.itemID == id);
        }

        public ItemSO GetItemByName(string name)
        {
            return allItems.Find(item => item.itemName == name);
        }
    }
}