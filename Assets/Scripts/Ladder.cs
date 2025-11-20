using UnityEngine;

namespace Sebbe
{
    public class Ladder : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                var pc = collision.GetComponent<PlayerController>();
                if (pc != null) pc.AddClimbContact();
            }
        }

        void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                var pc = collision.GetComponent<PlayerController>();
                if (pc != null) pc.RemoveClimbContact();
            }
        }
    }
}