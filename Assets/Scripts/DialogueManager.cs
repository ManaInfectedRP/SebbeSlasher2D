using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Sebbe
{
    // Simple singleton UI manager for branching yes/no conversations.
    // Set up a UI panel with the required references in the inspector.
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager instance;

        [Header("UI References")]
        public GameObject dialoguePanel; // root panel to enable/disable
        public TextMeshProUGUI dialogueText;
        public Button okButton;
        public Button noButton;
        [Tooltip("Container (RectTransform) where multiple option buttons will be instantiated")] public RectTransform optionsContainer;
        [Tooltip("Button prefab to use for option choices (should contain a TextMeshProUGUI child)")] public Button optionButtonPrefab;

        private NPC activeGuide;
        private DialogueNode[] nodes;
        private int currentIndex = -1;

        void Awake()
        {
            if (instance == null) instance = this;
            else if (instance != this) Destroy(gameObject);

            if (dialoguePanel != null) dialoguePanel.SetActive(false);

            if (okButton != null) okButton.onClick.AddListener(OnOkClicked);
            if (noButton != null) noButton.onClick.AddListener(OnNoClicked);
        }

        // Start a conversation with the provided guide and node list
        public void StartConversation(NPC guide, DialogueNode[] conversationNodes)
        {
            if (guide == null || conversationNodes == null || conversationNodes.Length == 0) return;
            activeGuide = guide;
            nodes = conversationNodes;
            currentIndex = 0;

            // Let the guide switch animator state
            activeGuide?.SetInConversation(true);

            ShowCurrentNode();
            if (dialoguePanel != null) dialoguePanel.SetActive(true);

            // Optionally prevent player movement here (not implemented)
        }

        private void ShowCurrentNode()
        {
            if (nodes == null || currentIndex < 0 || currentIndex >= nodes.Length)
            {
                EndConversation();
                return;
            }

            var node = nodes[currentIndex];
            if (dialogueText != null) dialogueText.text = node.text;

            // Clear existing option buttons
            if (optionsContainer != null)
            {
                for (int i = optionsContainer.childCount - 1; i >= 0; --i)
                {
                    Destroy(optionsContainer.GetChild(i).gameObject);
                }
            }

            // If this node defines options, show them as buttons and hide ok/no
            if (node.options != null && node.options.Length > 0 && optionsContainer != null && optionButtonPrefab != null)
            {
                if (okButton != null) okButton.gameObject.SetActive(false);
                if (noButton != null) noButton.gameObject.SetActive(false);

                for (int i = 0; i < node.options.Length; ++i)
                {
                    int optionIndex = i;
                    var btn = Instantiate(optionButtonPrefab, optionsContainer);
                    btn.gameObject.SetActive(true);
                    var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null) txt.text = node.options[optionIndex].text;
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OnOptionSelected(node.options[optionIndex].nextIndex));
                }
                return;
            }

            if (okButton != null) okButton.gameObject.SetActive(true);

            if (noButton != null)
            {
                if (node.hideNoButton)
                    noButton.gameObject.SetActive(false);
                else
                    noButton.gameObject.SetActive(true);
            }
        }

        private void OnOptionSelected(int nextIndex)
        {
            if (nextIndex < 0)
            {
                EndConversation();
            }
            else
            {
                currentIndex = Mathf.Clamp(nextIndex, 0, nodes.Length - 1);
                ShowCurrentNode();
            }
        }

        private void OnOkClicked()
        {
            if (nodes == null || currentIndex < 0) return;
            int next = nodes[currentIndex].yesIndex;
            if (next < 0)
            {
                EndConversation();
            }
            else
            {
                currentIndex = Mathf.Clamp(next, 0, nodes.Length - 1);
                ShowCurrentNode();
            }
        }

        private void OnNoClicked()
        {
            if (nodes == null || currentIndex < 0) return;
            int next = nodes[currentIndex].noIndex;
            if (next < 0)
            {
                EndConversation();
            }
            else
            {
                currentIndex = Mathf.Clamp(next, 0, nodes.Length - 1);
                ShowCurrentNode();
            }
        }

        private void EndConversation()
        {
            if (dialoguePanel != null) dialoguePanel.SetActive(false);
            if (activeGuide != null)
            {
                activeGuide.SetInConversation(false);
                activeGuide = null;
            }
            nodes = null;
            currentIndex = -1;

            // allow player input again if you blocked it earlier
        }
    }

        [System.Serializable]
        public struct DialogueOption
        {
            [TextArea(1, 2)] public string text;
            [Tooltip("Index of node to jump to when this option is selected; -1 ends conversation")] public int nextIndex;
        }

        [System.Serializable]
        public class DialogueNode
        {
            [TextArea] public string text;
            [Tooltip("Index of next node when player presses OK; -1 ends conversation")] public int yesIndex = -1;
            [Tooltip("Index of next node when player presses NO; -1 ends conversation")] public int noIndex = -1;
            [Tooltip("Hide the No button for this node (useful for informational steps)")] public bool hideNoButton = false;
            [Tooltip("Optional list of choices presented as buttons. Each choice's nextIndex determines which node to jump to.")]
            public DialogueOption[] options;
        }
}
