using UnityEngine;

namespace Sebbe
{
    public class SecretDoors : MonoBehaviour
    {
        [SerializeField] private GameObject openDoor;
        [SerializeField] private AudioClip doorOpenSound;
        [SerializeField] private float openDelay = 0.5f;
        [SerializeField] private KeyCode openDoorKey = KeyCode.E;

        [Header("Collider Settings")]
        [SerializeField] private Collider2D doorCollider;
        [SerializeField] private Collider2D triggerCollider;

        private bool isOpened = false;
        // Cooldown to avoid spamming floating text prompts while player stands in trigger
        private float promptCooldown = 1.5f;
        private float nextPromptTime = 0f;

        private void Start()
        {
            // Ensure the secret door is active and open door is inactive at start
            doorCollider.enabled = true;
            openDoor.SetActive(false);
        }

        private void Update()
        {
            bool playerInTrigger = IsPlayerInTrigger();

            // If player presses the configured key while in the trigger
            if (Input.GetKeyDown(openDoorKey) && !isOpened && playerInTrigger)
            {
                // If player has a key equipped, open the door
                if (Player.instance != null && Player.instance.hasEquippedKey)
                {
                    isOpened = true;
                    Invoke("OpenDoor", openDelay);
                }
                else
                {
                    // No key: show a floating text message informing the player
                    if (FloatingTextManager.instance != null)
                    {
                        Color c = Color.red;
                        Vector3 pos = Player.instance != null ? Player.instance.transform.position + Vector3.up * 1.2f : transform.position + Vector3.up * 1f;
                        FloatingTextManager.instance.Spawn("You need a key to open the door", pos, c, false, 2f);
                    }
                }
            }

            // If player is standing in the trigger and has a key, periodically show a prompt telling them which key to press
            if (playerInTrigger && !isOpened && Player.instance != null && Player.instance.hasEquippedKey)
            {
                if (Time.time >= nextPromptTime)
                {
                    nextPromptTime = Time.time + promptCooldown;
                    if (FloatingTextManager.instance != null)
                    {
                        Color c = Color.yellow;
                        Vector3 pos = Player.instance.transform.position + Vector3.up * 1.2f;
                        FloatingTextManager.instance.Spawn($"Press [{openDoorKey}] to open", pos, c, false, 1.5f);
                    }
                }
            }
        }

        private bool IsPlayerInTrigger()
        {
            Collider2D[] hits = Physics2D.OverlapBoxAll(triggerCollider.bounds.center, triggerCollider.bounds.size, 0f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    return true;
                }
            }
            return false;
        }

        private void OpenDoor()
        {
            // Play sound effect
            if (doorOpenSound != null)
            {
                AudioSource.PlayClipAtPoint(doorOpenSound, doorCollider.transform.position);
            }

            // Disable the secret door object
            doorCollider.enabled = false;
            // Enable the open door object
            openDoor.SetActive(true);
        }
        
    }
}