using UnityEngine;
using TMPro;

namespace Sebbe
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager instance;

        private Transform container;

        void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(gameObject);

            container = new GameObject("_FloatingTextContainer").transform;
            container.SetParent(this.transform, false);
        }

        // Spawns a temporary TextMeshPro floating text at world position
        public void Spawn(string text, Vector3 worldPos, Color color, bool isCrit = false, float duration = 1f)
        {
            GameObject go = new GameObject("FloatingText");
            go.transform.position = worldPos;
            go.transform.SetParent(container, true);

            var tmp = go.AddComponent<TextMeshPro>();
            var ft = go.AddComponent<FloatingText>();

            // Choose font size based on type
            float fontSize = isCrit ? 4f : 3f;
            // Slight upward velocity
            Vector3 vel = new Vector3(0f, .5f + (isCrit ? 0.3f : 0f), 0f);
            ft.Initialize(text, color, vel, duration, fontSize);
        }
    }
}
