using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadingPanel;

    [Header("Loading Bar")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Buttons")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;

    [Header("Fade")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    [Header("References")]
    [SerializeField] private GameStateController gameStateController;
    [SerializeField] private GridSpawner gridSpawner;

    [Header("Player Scripts To Disable During Menu")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private MonoBehaviour[] playerScriptsToDisable;

    private void Start()
    {
        SetPlayerInputActive(false);

        mainMenuPanel.SetActive(true);
        loadingPanel.SetActive(false);

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        newGameButton?.onClick.AddListener(OnNewGameClicked);
        loadGameButton?.onClick.AddListener(OnLoadGameClicked);
        settingsButton?.onClick.AddListener(OnSettingsClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);

        if (loadGameButton != null)
            loadGameButton.interactable = SaveManager.Instance.HasSave();
    }

    private void SetPlayerInputActive(bool active)
    {
        if (playerController != null)
            playerController.SetPlayerActive(active);

        foreach (var script in playerScriptsToDisable)
            if (script != null) script.enabled = active;
    }

    private void OnNewGameClicked()
    {
        gridSpawner.useRandomSeed = true;
        StartCoroutine(FadeThenSpawn(isLoad: false));
    }

    private void OnLoadGameClicked()
    {
        gridSpawner.useRandomSeed = false;
        StartCoroutine(FadeThenSpawn(isLoad: true));
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings clicked");
    }

    private void OnQuitClicked()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private IEnumerator FadeThenSpawn(bool isLoad)
    {
        // 1. Fade to black
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // 2. Swap to loading screen
        mainMenuPanel.SetActive(false);
        loadingPanel.SetActive(true);

        if (progressBar != null) progressBar.value = 0f;
        if (progressText != null) progressText.text = "0%";

        // 3. Fade back in to show loading screen
        if (fadeCanvasGroup != null)
        {
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = 1f - Mathf.Clamp01(t / fadeDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        // 4. Track progress on bar
        StartCoroutine(TrackSpawnProgress());

        // 5. Trigger spawning
        if (isLoad)
            gameStateController.LoadGame();
        else
            gameStateController.NewGame();

        // 6. Wait until spawning is fully complete
        yield return new WaitUntil(() =>
            gridSpawner.GetSpawningProgress() >= 1f && !gridSpawner.IsSpawning());

        // 7. Hide loading panel
        loadingPanel.SetActive(false);

        // 8. Re-enable the player — was incorrectly set to false before!
        SetPlayerInputActive(true);
    }

    private IEnumerator TrackSpawnProgress()
    {
        while (loadingPanel.activeSelf)
        {
            float progress = gridSpawner.GetSpawningProgress();

            if (progressBar != null)
                progressBar.value = progress;

            if (progressText != null)
                progressText.text = $"{(progress * 100):0}%";

            yield return null;
        }

        if (progressBar != null) progressBar.value = 1f;
        if (progressText != null) progressText.text = "100%";
    }

    private void OnDestroy()
    {
        newGameButton?.onClick.RemoveAllListeners();
        loadGameButton?.onClick.RemoveAllListeners();
        settingsButton?.onClick.RemoveAllListeners();
        quitButton?.onClick.RemoveAllListeners();
    }
}