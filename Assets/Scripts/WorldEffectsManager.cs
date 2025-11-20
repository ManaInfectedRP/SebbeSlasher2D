using UnityEngine;

namespace Sebbe
{
    public class WorldEffectsManager : MonoBehaviour
    {
        public static WorldEffectsManager instance;

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

        [Header("Global Effects")]
        public GameObject[] bloodSplatterPrefabs;
        public GameObject[] slimeDamageEffectsPrefabs;

        public void SpawnBloodSplatter(Vector2 position, Quaternion rotation)
        {
            if (bloodSplatterPrefabs.Length == 0) return;

            int index = Random.Range(0, bloodSplatterPrefabs.Length);
            Instantiate(bloodSplatterPrefabs[index], position, rotation);
        }

        public void SpawnSlimeDamageEffect(Vector2 position, Quaternion rotation)
        {
            if (slimeDamageEffectsPrefabs.Length == 0) return;

            int index = Random.Range(0, slimeDamageEffectsPrefabs.Length);
            Instantiate(slimeDamageEffectsPrefabs[index], position, rotation);
        }
    }
}
