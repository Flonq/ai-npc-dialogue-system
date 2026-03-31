using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    [SerializeField] private TMP_Text npcMessageText;
    [SerializeField] private string openingMessage = "Merhaba! Nasılsın?";

    private int npcLayer = -1;
    private bool canInteractWithNpc;
    private bool isDialogueOpen;

    public static bool IsDialogueOpen { get; private set; }

    private void Awake()
    {
        npcLayer = LayerMask.NameToLayer(npcLayerName);
        
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
                canInteractWithNpc = true;
        }

        if (promptHint != null)
            promptHint.SetActive(canInteractWithNpc && !isDialogueOpen);

        if (isDialogueOpen && Input.GetKeyDown(KeyCode.Escape))
            CloseDialogue();

        if (canInteractWithNpc && Input.GetKeyDown(KeyCode.E) && !isDialogueOpen)
            OpenDialogue();
    }

    private void OpenDialogue()
    {
        isDialogueOpen = true;
        IsDialogueOpen = true;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (npcMessageText != null && !string.IsNullOrEmpty(openingMessage))
            npcMessageText.text = openingMessage;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CloseDialogue()
    {
        isDialogueOpen = false;
        IsDialogueOpen = false;

        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}