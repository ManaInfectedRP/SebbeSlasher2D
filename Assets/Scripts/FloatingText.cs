using UnityEngine;
using TMPro;

namespace Sebbe
{
    [RequireComponent(typeof(TextMeshPro))]
    public class FloatingText : MonoBehaviour
    {
        private TextMeshPro tmp;
        private Vector3 velocity;
        private float lifeTime = 1f;
        private float elapsed = 0f;
        private Color startColor;

        public void Initialize(string text, Color color, Vector3 initialVelocity, float duration = 1f, float fontSize = 3f)
        {
            tmp = GetComponent<TextMeshPro>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            startColor = color;
            velocity = initialVelocity;
            lifeTime = Mathf.Max(0.01f, duration);
            elapsed = 0f;
        }

        void Awake()
        {
            tmp = GetComponent<TextMeshPro>();
            if (tmp == null) tmp = gameObject.AddComponent<TextMeshPro>();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            transform.position += velocity * dt;
            elapsed += dt;
            float t = Mathf.Clamp01(elapsed / lifeTime);
            // Fade out
            if (tmp != null)
            {
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                tmp.color = c;
            }
            if (elapsed >= lifeTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
