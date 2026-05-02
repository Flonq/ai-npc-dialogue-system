using UnityEngine;
using UnityEngine.UI;
using AINPC.AI;

namespace AINPC.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Camera")]
        [SerializeField] private Transform cameraTransform;

        [Header("Interaction")]
        [SerializeField] private float interactRange = 4f;
        [SerializeField] private string npcLayerName = "NPC";

        [Header("UI")]
        [SerializeField] private GameObject promptHint;
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject crosshair;

        [Header("Logic")]
        [SerializeField] private DialogueManager dialogueManager;

        private int npcLayer = -1;
        private bool canInteractWithNpc;
        private bool isDialogueOpen;
        private NpcCharacter activeNpc;

        public static bool IsDialogueOpen { get; private set; }

        private void Awake()
        {
            npcLayer = LayerMask.NameToLayer(npcLayerName);
            if (npcLayer == -1)
                Debug.LogError($"Layer not found: '{npcLayerName}'. Add it under Tags and Layers.");

            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;

            if (closeButton != null)
                closeButton.onClick.AddListener(CloseDialogue);

            IsDialogueOpen = false;
        }

        private void OnDestroy()
        {
            if (closeButton != null)
                closeButton.onClick.RemoveListener(CloseDialogue);
        }

        private void Update()
        {
            canInteractWithNpc = false;
            activeNpc = null;

            if (cameraTransform == null || npcLayer == -1)
            {
                if (promptHint != null)
                    promptHint.SetActive(false);
                return;
            }

            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactRange))
            {
                if (hit.collider != null && hit.collider.gameObject.layer == npcLayer)
                {
                    activeNpc = hit.collider.GetComponentInParent<NpcCharacter>();
                    canInteractWithNpc = activeNpc != null && activeNpc.Profile != null;
                }
            }

            if (promptHint != null)
                promptHint.SetActive(canInteractWithNpc && !isDialogueOpen);

            if (canInteractWithNpc && Input.GetKeyDown(KeyCode.E) && !isDialogueOpen)
                OpenDialogue();
        }

        private void OpenDialogue()
        {
            if (activeNpc == null || activeNpc.Profile == null)
            {
                Debug.LogWarning("[PlayerInteraction] No NpcCharacter/Profile found on target.");
                return;
            }

            isDialogueOpen = true;
            IsDialogueOpen = true;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(true);

            if (dialogueManager != null)
                dialogueManager.StartDialogue(activeNpc.Profile);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (crosshair != null)
                crosshair.SetActive(false);
        }

        private void CloseDialogue()
        {
            isDialogueOpen = false;
            IsDialogueOpen = false;

            if (dialoguePanel != null)
                dialoguePanel.SetActive(false);

            if (dialogueManager != null)
                dialogueManager.EndDialogue();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (crosshair != null)
                crosshair.SetActive(true);
        }

        public bool TryConsumeEscapeForDialogue()
        {
            if (!isDialogueOpen)
                return false;
            CloseDialogue();
            return true;
        }
    }
}