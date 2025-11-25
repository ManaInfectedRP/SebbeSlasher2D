using UnityEngine;
using TMPro;

namespace Sebbe
{
    public class Guide : NPC
    {
        [Header("Interaction")]
        public float interactRadius = 2f;
        public KeyCode interactKey = KeyCode.E;
        [Header("Prompt")]
        [Tooltip("Optional prefab for the 'Press [E] to talk' prompt. If unset a simple TextMeshPro object is created at runtime.")]
        public GameObject promptPrefab;
        [Tooltip("Local offset for the prompt when instantiated as a child of the Guide.")]
        public Vector3 promptLocalOffset = new Vector3(0f, 1.4f, 0f);

        [Header("Animator")]
        public Animator animator;
        [Tooltip("Animator bool name to set while in conversation")]
        public string inConversationParam = "inConversation";

        [System.Serializable]
        public class GuideConversation
        {
            public string title;
            public DialogueNode[] nodes;
        }

        [Header("Conversation")]
        [Tooltip("One or more named conversations this Guide can present. If multiple are present the Guide will show a selection menu.")]
        public GuideConversation[] conversations;

        private Transform playerT;
        private GameObject promptInstance;
        private bool inConversation = false;

        private void Update()
        {
            if (Player.instance != null) playerT = Player.instance.transform;
            if (playerT == null) return;

            float dist = Vector2.Distance(transform.position, playerT.position);
            if (dist <= interactRadius)
            {
                // If player presses interact key, start conversation
                if (!inConversation)
                {
                    EnsurePromptVisible();
                    if (Input.GetKeyDown(interactKey))
                    {
                        StartConversation();
                    }
                }
            }
            else
            {
                EnsurePromptHidden();
            }
        }

        public void StartConversation()
        {
            if ((conversations == null || conversations.Length == 0)) return;
            inConversation = true;
            EnsurePromptHidden();

            // If there is only one conversation, start it directly
            if (conversations.Length == 1)
            {
                var only = conversations[0];
                if (only == null || only.nodes == null || only.nodes.Length == 0) return;
                DialogueManager.instance?.StartConversation(this, only.nodes);
                return;
            }

            // Build a combined node list: a selection node first, then all conversation nodes concatenated, then a farewell node.
            var combined = new System.Collections.Generic.List<DialogueNode>();

            // Selection node placeholder
            var selectionNode = new DialogueNode() { text = "What would you like to hear?" };
            // create options later once we know offsets
            selectionNode.options = new DialogueOption[conversations.Length];
            combined.Add(selectionNode);

            // Track start indices for each conversation
            var starts = new System.Collections.Generic.List<int>();
            for (int c = 0; c < conversations.Length; ++c)
            {
                var conv = conversations[c];
                starts.Add(combined.Count);
                if (conv != null && conv.nodes != null)
                {
                    for (int n = 0; n < conv.nodes.Length; ++n)
                    {
                        // copy node to avoid modifying inspector assets
                        var src = conv.nodes[n];
                        var copy = new DialogueNode()
                        {
                            text = src.text,
                            yesIndex = src.yesIndex,
                            noIndex = src.noIndex,
                            hideNoButton = src.hideNoButton,
                            options = src.options
                        };
                        // adjust internal indices by conversation start offset (we'll adjust to absolute indices below)
                        if (copy.yesIndex >= 0) copy.yesIndex += starts[c];
                        if (copy.noIndex >= 0) copy.noIndex += starts[c];
                        // if the source node had options, their nextIndex also needs offset; adjust if present
                        if (copy.options != null)
                        {
                            for (int oi = 0; oi < copy.options.Length; ++oi)
                            {
                                var o = copy.options[oi];
                                if (o.nextIndex >= 0) o.nextIndex += starts[c];
                                copy.options[oi] = o;
                            }
                        }
                        combined.Add(copy);
                    }
                }
            }

            // Farewell node appended at end
            int farewellIndex = combined.Count;
            var farewell = new DialogueNode() { text = "Good luck on your journey!", yesIndex = -1, noIndex = -1, hideNoButton = true };
            combined.Add(farewell);

            // Fix last-node behavior: for each conversation, if its last node had yesIndex == -1 originally, point it to farewell
            for (int c = 0; c < conversations.Length; ++c)
            {
                var conv = conversations[c];
                if (conv == null || conv.nodes == null || conv.nodes.Length == 0) continue;
                int convStart = starts[c];
                int lastIdxInConv = conv.nodes.Length - 1;
                var srcLast = conv.nodes[lastIdxInConv];
                int combinedIdx = convStart + lastIdxInConv;
                if (srcLast.yesIndex == -1)
                {
                    combined[combinedIdx].yesIndex = farewellIndex;
                }
                if (srcLast.noIndex == -1)
                {
                    combined[combinedIdx].noIndex = farewellIndex;
                }
            }

            // Now fill selection node options to jump to the start of each conversation
            for (int c = 0; c < conversations.Length; ++c)
            {
                string title = conversations[c] != null && !string.IsNullOrEmpty(conversations[c].title) ? conversations[c].title : $"Topic {c + 1}";
                selectionNode.options[c] = new DialogueOption() { text = title, nextIndex = starts[c] };
            }

            // Start the combined conversation
            DialogueManager.instance?.StartConversation(this, combined.ToArray());
        }

        // Called by DialogueManager to switch animator state
        public override void SetInConversation(bool v)
        {
            if (animator != null && !string.IsNullOrEmpty(inConversationParam))
            {
                animator.SetBool(inConversationParam, v);
            }
            inConversation = v;
            if (!v) EnsurePromptHidden();
        }

        private void EnsurePromptVisible()
        {
            if (promptInstance != null) return;

            if (promptPrefab != null)
            {
                promptInstance = Instantiate(promptPrefab, transform);
                promptInstance.transform.localPosition = promptLocalOffset;
                promptInstance.transform.localRotation = Quaternion.identity;
            }
            else
            {
                // create a simple TextMeshPro floating label
                promptInstance = new GameObject("GuidePrompt", typeof(TextMeshPro));
                var tmp = promptInstance.GetComponent<TextMeshPro>();
                tmp.text = $"Press [{interactKey}] to talk";
                tmp.fontSize = 3f;
                tmp.color = Color.yellow;
                // ensure it renders above sprites
                var rend = tmp.GetComponent<Renderer>();
                if (rend != null) rend.sortingOrder = 99;
                promptInstance.transform.SetParent(transform, false);
                promptInstance.transform.localPosition = promptLocalOffset;
                promptInstance.transform.localRotation = Quaternion.identity;
            }

            // Make sure the prompt doesn't block clicks
            var cg = promptInstance.GetComponent<CanvasGroup>();
            if (cg == null) cg = promptInstance.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
        }

        private void EnsurePromptHidden()
        {
            if (promptInstance == null) return;
            Destroy(promptInstance);
            promptInstance = null;
        }

        private void Awake()
        {
            // If no conversations assigned in inspector, create a small sample conversation for quick testing
            if (conversations == null || conversations.Length == 0)
            {
                conversations = new GuideConversation[2];

                conversations[0] = new GuideConversation();
                conversations[0].title = "General Hint";
                conversations[0].nodes = new DialogueNode[3];
                conversations[0].nodes[0] = new DialogueNode() { text = "This will be unraveling..", yesIndex = 1, noIndex = -1 };
                conversations[0].nodes[1] = new DialogueNode() { text = "There are Keys hidden that opens secret Doors!", yesIndex = 2, noIndex = -1 };
                conversations[0].nodes[2] = new DialogueNode() { text = "Good luck on your journey!", yesIndex = -1, noIndex = -1, hideNoButton = true };

                conversations[1] = new GuideConversation();
                conversations[1].title = "Secrets";
                conversations[1].nodes = new DialogueNode[2];
                conversations[1].nodes[0] = new DialogueNode() { text = "Psst — look for a cracked wall near the river.", yesIndex = 1, noIndex = -1 };
                conversations[1].nodes[1] = new DialogueNode() { text = "That crack hides a short tunnel to a treasure.", yesIndex = -1, noIndex = -1, hideNoButton = true };
            }
        }
    }
}