using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Sebbe
{
    public class AchivementEntryUI : MonoBehaviour
    {
        public Image iconImage;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI progressText;
        public GameObject achivementLockedOverlay;


        // currentValue is passed separately so we do not modify the ScriptableObject asset at runtime
        public void Setup(AchivementSO achivement, bool unlocked, int currentValue = 0)
        {
            if (achivementLockedOverlay != null)
            {
                achivementLockedOverlay.SetActive(!unlocked);
            }
            if (iconImage != null)
            {
                iconImage.sprite = achivement.icon;
            }
            if (nameText != null)
            {
                nameText.text = achivement.achivementName;
            }
            if (descriptionText != null)
            {
                descriptionText.text = achivement.description;
            }
            // Show progress as "current / required" when not unlocked and when a requirement exists
            if (progressText != null)
            {
                if (achivement.requiredValue > 0 && !unlocked)
                {
                    progressText.gameObject.SetActive(true);
                    progressText.text = string.Format("{0} / {1}", currentValue, achivement.requiredValue);
                }
                else
                {
                    progressText.gameObject.SetActive(false);
                }
            }
        }
        
    }
}
