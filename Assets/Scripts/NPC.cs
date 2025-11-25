using UnityEngine;

namespace Sebbe
{
    public class NPC : MonoBehaviour
    {

        // Default hook for conversation state. Derived NPCs (like Guide) can override this
        public virtual void SetInConversation(bool v)
        {
            // default no-op
        }

    }
}
