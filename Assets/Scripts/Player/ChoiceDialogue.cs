using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChoiceDialogue : MonoBehaviour
{
    [SerializeField] private TMP_Text npcMessageText;
    [SerializeField] private Button[] choiceButtons;

    [Header("Lines (English)")]
    [TextArea(2, 4)]
    [SerializeField] private string npcQuestion = "How are you?";

    [SerializeField] private string[] choiceLabels = { "Fine", "Tired", "I need to go" };
    [SerializeField] private string[] choiceResponses = { "Glad to hear!", "Get some rest.", "Goodbye." };

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

    public void OnDialogueOpened()
    {
        if (npcMessageText != null)
            npcMessageText.text = npcQuestion;

        if (choiceButtons == null)
            return;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            bool show = choiceLabels != null && i < choiceLabels.Length && !string.IsNullOrEmpty(choiceLabels[i]);
            if (choiceButtons[i] == null)
                continue;

            choiceButtons[i].gameObject.SetActive(show);

            if (!show)
                continue;

            TMP_Text label = choiceButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = choiceLabels[i];
        }
    }

    private void OnChoiceClicked(int index)
    {
        if (choiceResponses == null || index < 0 || index >= choiceResponses.Length)
            return;

        if (npcMessageText != null)
            npcMessageText.text = choiceResponses[index];

        if (choiceButtons == null)
            return;

        foreach (Button b in choiceButtons)
        {
            if (b != null)
                b.gameObject.SetActive(false);
        }
    }
}