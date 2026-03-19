using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuHandler : MonoBehaviour
{
    [Header("References")]
    public BuildingSaveSystem buildingSaveSystem;

    [Header("Player")]
    public PlayerController playerController;
    public MonoBehaviour[] playerScriptsToDisable;

    [Header("Pause Menu Panel")]
    public GameObject pauseMenuPanel;

    [Header("UI Buttons")]
    public Button saveButton;
    public Button loadButton;
    public Button resumeButton;
    public Button quitButton;
    
    [Header("Feedback Text (Optional)")]
    public TextMeshProUGUI feedbackText;
    public float feedbackDuration = 2f;

    private bool isPaused = false;
    private Coroutine feedbackCoroutine;

    private void Start()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(OnSaveClicked);
        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadClicked);
        if (resumeButton != null)
            resumeButton.onClick.AddListener(OnResumeClicked);
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
    }

    private void Update()
    {
        // Don't allow pausing before the player is active
        if (playerController != null && !playerController.enabled) return;

        if (Input.GetKeyDown(KeyCode.P) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        // Unlock cursor for UI
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Disable player input scripts
        SetPlayerScriptsEnabled(false);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        // Re-lock cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Re-enable player input scripts
        SetPlayerScriptsEnabled(true);

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        ClearFeedback();
    }

    private void SetPlayerScriptsEnabled(bool enabled)
    {
        if (playerController != null)
            playerController.enabled = enabled;

        foreach (var script in playerScriptsToDisable)
            if (script != null) script.enabled = enabled;
    }

    private void OnSaveClicked()
    {
        if (buildingSaveSystem == null) { ShowFeedback("Error: No BuildingSaveSystem assigned!"); return; }
        buildingSaveSystem.SaveBuilding();
        ShowFeedback("Building saved successfully!");
    }

    private void OnLoadClicked()
    {
        if (buildingSaveSystem == null) { ShowFeedback("Error: No BuildingSaveSystem assigned!"); return; }
        buildingSaveSystem.LoadBuilding();
        ShowFeedback("Building loaded successfully!");
    }

    private void OnResumeClicked()
    {
        ResumeGame();
    }

    private void OnQuitClicked()
    {
        Time.timeScale = 1f;
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private void ShowFeedback(string message)
    {
        if (feedbackText == null) return;
        feedbackText.text = message;
        feedbackText.gameObject.SetActive(true);
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);
        feedbackCoroutine = StartCoroutine(ClearFeedbackAfterDelay());
    }

    private void ClearFeedback()
    {
        if (feedbackText != null)
            feedbackText.gameObject.SetActive(false);
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }
    }

    private System.Collections.IEnumerator ClearFeedbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);
        ClearFeedback();
    }
}