using UnityEngine;

namespace Sebbe
{
    [CreateAssetMenu(fileName = "New Achivement", menuName = "Achivement")]
    public class AchivementSO : ScriptableObject
    {
        public int id;
        public string achivementName;
        public string description;
        public Sprite icon;


        [Header("Requirements")]
        public int requiredValue; // e.g., number of enemies to defeat
        public int currentValue; // e.g., current progress towards the achievement
    }
}