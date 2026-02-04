using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Main Gameplay";
    
    private bool isTransitioning = false;

    private void Start()
    {
        // Setup button listeners
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        
        if (quitButton != null)
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        
        // Ensure fade panel starts transparent
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnStartButtonClicked()
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoadSceneWithFade(gameplaySceneName));
        }
    }

    private void OnSettingsButtonClicked()
    {
        // Add your settings menu logic here
        Debug.Log("Settings button clicked!");
        // Example: Open settings panel, load settings scene, etc.
    }

    private void OnQuitButtonClicked()
    {
        Debug.Log("Quit button clicked!");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        isTransitioning = true;
        
        // Enable raycasts blocking during transition
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.blocksRaycasts = true;
        
        // Fade out
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
        
        // Ensure fade is complete
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.alpha = 1f;
        
        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }
}