using UnityEngine;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private GameObject crosshair;

    public static bool IsPaused { get; private set; }

    private void Awake()
    {
        if (pausePanel != null)
            pausePanel.SetActive(false);
        IsPaused = false;

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeGame);
        if (quitButton != null)
            quitButton.onClick.RemoveListener(QuitGame);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (playerInteraction != null && playerInteraction.TryConsumeEscapeForDialogue())
            return;

        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    private void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        if (pausePanel != null)
            pausePanel.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (crosshair != null)
            crosshair.SetActive(false);
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        if (pausePanel != null)
            pausePanel.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (crosshair != null)
            crosshair.SetActive(true);
    }

    private void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}