using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PauseMenuHandler : MonoBehaviour
{
    [Header("References")]
    public BuildingSaveSystem buildingSaveSystem;

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

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitClicked);
        }

    private void Update()
    {
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

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);

        ClearFeedback();
    }

    private void OnSaveClicked()
    {
        if (buildingSaveSystem == null)
        {
            ShowFeedback("Error: No BuildingSaveSystem assigned!");
            return;
        }

        buildingSaveSystem.SaveBuilding();
        ShowFeedback("Building saved successfully!");
    }

    private void OnLoadClicked()
    {
        if (buildingSaveSystem == null)
        {
            ShowFeedback("Error: No BuildingSaveSystem assigned!");
            return;
        }

        buildingSaveSystem.LoadBuilding();
        ShowFeedback("Building loaded successfully!");
    }

    private void OnResumeClicked()
    {
        ResumeGame();
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

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
}

    private System.Collections.IEnumerator ClearFeedbackAfterDelay()
    {
        yield return new WaitForSecondsRealtime(feedbackDuration);
        ClearFeedback();
    }
}