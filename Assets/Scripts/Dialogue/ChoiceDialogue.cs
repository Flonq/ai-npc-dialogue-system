using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceDialogue : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text npcMessageText;
    [SerializeField] private Button[] choiceButtons;

    [Header("Logic")]
    [SerializeField] private DialogueManager dialogueManager;

    [Header("Loading")]
    [SerializeField] private string loadingText = "...";

    private void Awake()
    {
        if (choiceButtons == null)
            return;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;
            if (choiceButtons[i] != null)
                choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(index));
        }
    }

    private void OnDestroy()
    {
        if (choiceButtons == null)
            return;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] != null)
                choiceButtons[i].onClick.RemoveAllListeners();
        }
    }

    public void ShowLoading()
    {
        if (npcMessageText != null)
            npcMessageText.text = loadingText;

        if (choiceButtons == null)
            return;

        foreach (Button b in choiceButtons)
        {
            if (b == null)
                continue;

            b.gameObject.SetActive(true);
            b.interactable = false;

            TMP_Text label = b.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = loadingText;
        }
    }

    public void SetTurn(string npcReply, string[] choices)
    {
        if (npcMessageText != null)
            npcMessageText.text = npcReply ?? "";

        if (choiceButtons == null)
            return;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            if (choiceButtons[i] == null)
                continue;

            bool show = choices != null && i < choices.Length && !string.IsNullOrEmpty(choices[i]);
            choiceButtons[i].gameObject.SetActive(show);

            if (!show)
                continue;

            choiceButtons[i].interactable = true;

            TMP_Text label = choiceButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = choices[i];
        }
    }

    private void OnChoiceClicked(int index)
    {
        if (dialogueManager == null)
            return;

        if (choiceButtons != null)
        {
            foreach (Button b in choiceButtons)
            {
                if (b != null)
                    b.interactable = false;
            }
        }

        dialogueManager.OnPlayerChose(index);
    }
}