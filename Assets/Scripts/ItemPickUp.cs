using UnityEngine;

namespace Sebbe
{
    // Generic item pickup. Assign an `ItemSO` in the inspector to control what the player picks up.
    public class ItemPickUp : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] float bounceHeight = 0.25f;
        [SerializeField] float rotationSpeed = 50f;
        [SerializeField] private Vector3 initialPosition;

        [Header("Item")]
        [Tooltip("The ItemSO this pickup grants to the player when collected.")]
        [SerializeField] private ItemSO item;

        [Header("Optional Achievement")]
        [Tooltip("Optional achievement index to increment on pickup. Set to -1 to skip.")]
        [SerializeField] private int achievementID = -1;
        [Tooltip("How much progress to add for the achievement when picked up.")]
        [SerializeField] private int achievementProgressAmount = 1;

        void Start()
        {
            if (initialPosition == Vector3.zero)
                initialPosition = transform.position;
        }

        void Update()
        {
            // Bounce effect
            float newY = Mathf.Sin(Time.time * 2f) * bounceHeight;
            transform.position = new Vector3(transform.position.x, newY + initialPosition.y, transform.position.z);

            // Rotate effect
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<Sebbe.Player>();
            if (player != null)
            {
                if (item == null)
                {
                    Debug.LogWarning("ItemPickUp has no ItemSO assigned.");
                    return;
                }

                // If this is a weapon, mark that the player has obtained it (does not auto-equip)
                if (item.isWeapon)
                {
                    player.FoundSword();
                }

                // Add the item to the player's inventory
                if (player.inventory != null)
                {
                    player.inventory.AddItem(item.itemID);
                }

                // Optional achievements
                if (achievementID >= 0 && WorldAchivementManager.instance != null)
                {
                    WorldAchivementManager.instance.AddProgress(achievementID, achievementProgressAmount);
                }

                Destroy(gameObject);
            }
        }
    }
}