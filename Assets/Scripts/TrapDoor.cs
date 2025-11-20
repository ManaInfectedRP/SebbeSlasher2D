using System.Collections;
using UnityEngine;

namespace Sebbe
{
    public class TrapDoor : MonoBehaviour
    {
        [HideInInspector] public Collider2D collider2d;
        [HideInInspector] public Animation anim;

        [SerializeField] private float openDuration = 2f;
        [SerializeField] private bool openDelay;
        [SerializeField] private float closeDelay = 2f;

        void Awake()
        {
            collider2d = GetComponent<Collider2D>();
            anim = GetComponent<Animation>();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.CompareTag("Player"))
            {
                if (openDelay)
                {
                    openDuration -= Time.deltaTime;
                    if (openDuration <= 0f)
                    {
                        Open();
                    }
                }
                else
                {
                    Open();
                }
                
                StartCoroutine(CloseAfterDelay(closeDelay));
            }
        }

        public void Open()
        {
            if (anim != null)
            {

                anim.Play("TrapDoor_Open");
            }
        }

        IEnumerator CloseAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (anim != null)
            {
                anim.Play("TrapDoor_Close");
            }
        }
    }
}