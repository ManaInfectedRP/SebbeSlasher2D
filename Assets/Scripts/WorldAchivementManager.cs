using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Sebbe
{
    public class WorldAchivementManager : MonoBehaviour
    {
        public static WorldAchivementManager instance;

        public AchivementSO[] achivements;
        public bool[] achivementUnlocked;
        // runtime progress so we don't modify the ScriptableObject assets
        public int[] achivementProgress;

        [Header("Pop-Up UI Elements")]
        public GameObject achivementPopupPanel;
        public Image achivementIcon;
        public TextMeshProUGUI achivementNameText;
        public TextMeshProUGUI achivementDescriptionText;

        [Header("Achivement List UI")]
        public GameObject achivementPanel;
        public Transform achivementListContent;
        public GameObject achivementPrefab;


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
            int len = achivements != null ? achivements.Length : 0;
            achivementUnlocked = new bool[len];
            achivementProgress = new int[len];
            for (int i = 0; i < len; i++)
            {
                achivementUnlocked[i] = false;
                achivementProgress[i] = 0;
            }
            // Pre-populate the achievement list so entries exist in the UI (even if panel is hidden)
            if (achivementListContent != null && achivementPrefab != null)
            {
                PopulateAchivementList();
                // start hidden by default
                if (achivementPanel != null) achivementPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                achivementPanel.SetActive(!achivementPanel.activeSelf);
                if (achivementPanel.activeSelf)
                {
                    PopulateAchivementList();
                }
            }
        }

        private void PopulateAchivementList()
        {
            // Clear existing entries
            foreach (Transform child in achivementListContent)
            {
                Destroy(child.gameObject);
            }

            // Populate with current achivements
            for (int i = 0; i < achivements.Length; i++)
            {
                GameObject entry = Instantiate(achivementPrefab, achivementListContent);
                AchivementEntryUI entryUI = entry.GetComponent<AchivementEntryUI>();
                if (entryUI != null)
                {
                    int progress = (achivementProgress != null && i < achivementProgress.Length) ? achivementProgress[i] : 0;
                    entryUI.Setup(achivements[i], achivementUnlocked[i], progress);
                }
                
            }
        }

        public void UnlockAchivement(int id)
        {
            if (id < 0 || id >= achivementUnlocked.Length) return;
            if (!achivementUnlocked[id])
            {
                achivementUnlocked[id] = true;
                if (achivementProgress != null && id < achivementProgress.Length)
                {
                    achivementProgress[id] = achivements[id] != null ? achivements[id].requiredValue : achivementProgress[id];
                }
                Debug.Log("Achivement Unlocked: " + achivements[id].achivementName);
                StartCoroutine(ShowAchivementPopUpUI(achivements[id]));
                // If the achievement list UI is populated, refresh the specific entry overlay
                if (achivementListContent != null && id < achivementListContent.childCount)
                {
                    Transform entry = achivementListContent.GetChild(id);
                    if (entry != null)
                    {
                        var entryUI = entry.GetComponent<AchivementEntryUI>();
                        if (entryUI != null)
                        {
                            int progress = (achivementProgress != null && id < achivementProgress.Length) ? achivementProgress[id] : 0;
                            entryUI.Setup(achivements[id], true, progress);
                        }
                    }
                }
            }
        }

        // Increment progress for achievement with given id. If progress reaches requiredValue, unlock it.
        public void AddProgress(int id, int amount = 1)
        {
            if (id < 0 || id >= achivements.Length) return;

            var so = achivements[id];
            if (so == null) return;
            // Update runtime progress instead of modifying the ScriptableObject asset
            if (achivementProgress == null || id >= achivementProgress.Length) return;
            achivementProgress[id] = Mathf.Clamp(achivementProgress[id] + amount, 0, so.requiredValue);

            // If reached required value, unlock
            if (achivementProgress[id] >= so.requiredValue)
            {
                UnlockAchivement(id);
            }
            else
            {
                // If UI is populated, refresh the specific entry to show progress
                if (achivementListContent != null && id < achivementListContent.childCount)
                {
                    Transform entry = achivementListContent.GetChild(id);
                    if (entry != null)
                    {
                        var entryUI = entry.GetComponent<AchivementEntryUI>();
                        if (entryUI != null)
                        {
                            entryUI.Setup(achivements[id], achivementUnlocked[id], achivementProgress[id]);
                        }
                    }
                }
            }
        }

        private IEnumerator ShowAchivementPopUpUI(AchivementSO achivement)
        {
            yield return new WaitForSeconds(0.1f);
            achivementPopupPanel.SetActive(true);

            if(achivementIcon != null){
                achivementIcon.sprite = achivement.icon;
                achivementIcon.gameObject.SetActive(achivement.icon != null);
            }
            
            achivementNameText.text = achivement.achivementName;
            achivementDescriptionText.text = achivement.description;
            yield return new WaitForSeconds(3f);
            achivementPopupPanel.SetActive(false);
        }
    }
} 