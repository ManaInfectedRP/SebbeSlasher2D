using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sebbe
{
    public class WorldStatsManager : MonoBehaviour
    {
        public static WorldStatsManager instance;

        [Header("Player")]
        public Player player;

        [Header("UI References")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Slider manaSlider;
        [SerializeField] private Slider damageReductionSlider;

        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI manaText;
        [SerializeField] private TextMeshProUGUI staminaText;
        [SerializeField] private TextMeshProUGUI damageReductionText;

        [SerializeField] private TextMeshProUGUI currentAttackDamageText;
        [SerializeField] private TextMeshProUGUI currentAttackRangeText;
        [SerializeField] private TextMeshProUGUI currentAttackRateText;

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

        void Start()
        {
            player = Player.instance;
            UpdatePlayerUI();
        }

        public void UpdatePlayerUI()
        {
            if (player == null) return;

            if (healthSlider != null)
            {
                healthSlider.maxValue = player.GetMaxHealth();
                healthSlider.value = player.GetCurrentHealth();
            }

            if (manaSlider != null)
            {
                manaSlider.maxValue = player.GetMaxMana();
                manaSlider.value = player.GetCurrentMana();
            }
            if (damageReductionSlider != null)
            {
                damageReductionSlider.maxValue = 100f;
                float drVal = 0f;
                if (EquipmentManager.instance != null)
                    drVal = EquipmentManager.instance.damageReduction;
                else if (player != null)
                    drVal = player.damageReductionPercent;
                damageReductionSlider.value = drVal;
            }

            if (healthText != null)
            {
                // Show current health / max health as whole numbers
                int cur = Mathf.RoundToInt(player.GetCurrentHealth());
                int max = Mathf.RoundToInt(player.GetMaxHealth());
                healthText.text = $"{cur} / {max}";
            }
            if (manaText != null)
            {
                manaText.text = $"{player.GetCurrentMana()} / {player.GetMaxMana()}";
            }
            if (staminaText != null)
            {
                // Show damage reduction as a 0/100 value in the staminaText slot
                float drVal = 0f;
                if (EquipmentManager.instance != null)
                    drVal = EquipmentManager.instance.damageReduction;
                else
                    drVal = player.damageReductionPercent;

                staminaText.text = $"{Mathf.RoundToInt(drVal)} / 100";
            }

            if (currentAttackDamageText != null)
            {
                currentAttackDamageText.text = $"Damage: {player.GetDamage()}";
            }

            if (currentAttackRangeText != null)
            {
                currentAttackRangeText.text = $"Range: {player.GetAttackRange()}";
            }

            if (currentAttackRateText != null)
            {
                currentAttackRateText.text = $"Attack Rate: {player.GetAttackRate()}";
            }

            if (damageReductionText != null)
            {
                float dr = 0f;
                if (EquipmentManager.instance != null)
                {
                    dr = EquipmentManager.instance.damageReduction;
                }
                else if (player != null)
                {
                    dr = player.damageReductionPercent;
                }
                damageReductionText.text = $"DMG Reduction: {dr.ToString("F1")} %";
            }
        }

        void Update()
        {
            UpdatePlayerUI();
            UpdateAttackStatsUI();
        }

        public void UpdateHealthUI()
        {
            if (player == null) return;

            if (healthSlider != null)
            {
                healthSlider.value = player.GetCurrentHealth();
            }

            if (healthText != null)
            {
                int cur = Mathf.RoundToInt(player.GetCurrentHealth());
                int max = Mathf.RoundToInt(player.GetMaxHealth());
                healthText.text = $"{cur} / {max}";
            }
        }

        public void UpdateManaUI()
        {
            if (player == null) return;

            if (manaSlider != null)
            {
                manaSlider.value = player.GetCurrentMana();
            }

            if (manaText != null)
            {
                manaText.text = $"{player.GetCurrentMana()} / {player.GetMaxMana()}";
            }
        }

        public void UpdateStaminaUI()
        {
            if (player == null) return;

            float dr = 0f;
            if (EquipmentManager.instance != null)
            {
                dr = EquipmentManager.instance.damageReduction;
            }
            else
            {
                dr = player.damageReductionPercent;
            }

            if (damageReductionSlider != null)
            {
                damageReductionSlider.value = dr;
            }

            if (damageReductionText != null)
            {
                damageReductionText.text = $"DMG Reduction: {dr.ToString("F1")} %";
            }
        }

        public void UpdateAttackStatsUI()
        {
            if (player == null) return;

            if (currentAttackDamageText != null)
            {
                currentAttackDamageText.text = $"Damage: {player.GetDamage()}";
            }

            if (currentAttackRangeText != null)
            {
                currentAttackRangeText.text = $"Range: {player.GetAttackRange()}";
            }

            if (currentAttackRateText != null)
            {
                currentAttackRateText.text = $"Attack Rate: {player.GetAttackRate()}";
            }
        }
    }
}
