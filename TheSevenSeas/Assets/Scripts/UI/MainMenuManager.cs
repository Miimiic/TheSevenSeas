using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button quitButton;
    
    [Header("Fade Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("Loading Screen")]
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI debugText; // NEW: Add this for debug info
    [SerializeField] private float minimumLoadTime = 1f;
    
    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "Main Gameplay";
    
    [Header("Debug")]
    [SerializeField] private bool enableDebugLogging = true;
    
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
        
        // Ensure loading screen starts hidden
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }
    
    private void OnStartButtonClicked()
    {
        if (!isTransitioning)
        {
            StartCoroutine(LoadSceneWithFadeAndLoading(gameplaySceneName));
        }
    }
    
    private void OnSettingsButtonClicked()
    {
        Debug.Log("Settings button clicked!");
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
    
    private IEnumerator LoadSceneWithFadeAndLoading(string sceneName)
    {
        isTransitioning = true;
        
        DebugLog("=== LOADING STARTED ===");
        DebugLog($"Target scene: {sceneName}");
        
        // Enable raycasts blocking during transition
        if (fadeCanvasGroup != null)
            fadeCanvasGroup.blocksRaycasts = true;
        
        DebugLog("Starting fade out...");
        
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
        
        DebugLog("Fade out complete");
        
        // Show loading screen
        if (loadingScreen != null)
            loadingScreen.SetActive(true);
        
        DebugLog("Loading screen shown");
        
        // Small delay for visual transition
        yield return new WaitForSeconds(0.2f);
        
        DebugLog("Starting async scene load...");
        
        // Start async loading
        float startTime = Time.time;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;
        
        DebugLog("AsyncOperation created");
        
        float lastProgress = -1f;
        int frameCount = 0;
        float lastFrameTime = Time.time;
        
        // Update progress bar while loading
        while (!operation.isDone)
        {
            frameCount++;
            float currentTime = Time.time;
            float deltaTime = currentTime - lastFrameTime;
            lastFrameTime = currentTime;
            
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            
            // Log when progress changes
            if (Mathf.Abs(progress - lastProgress) > 0.01f)
            {
                DebugLog($"Progress: {progress * 100:F2}% | Raw: {operation.progress:F3} | Frame: {frameCount} | Delta: {deltaTime:F3}s");
                lastProgress = progress;
            }
            
            // Update UI
            if (progressBar != null)
                progressBar.value = progress;
            
            if (progressText != null)
                progressText.text = $"{(progress * 100):0}%";
            
            // Update debug text on screen
            if (debugText != null)
            {
                debugText.text = $"Progress: {progress * 100:F1}%\n" +
                                $"Raw Progress: {operation.progress:F3}\n" +
                                $"Frame: {frameCount}\n" +
                                $"Time: {(currentTime - startTime):F2}s\n" +
                                $"FPS: {(1f / deltaTime):F0}";
            }
            
            // Check if loading is complete
            if (operation.progress >= 0.9f)
            {
                DebugLog($"Scene loaded! Total time: {Time.time - startTime:F2}s, Frames: {frameCount}");
                
                // Wait for minimum load time if needed
                float loadElapsedTime = Time.time - startTime;
                if (loadElapsedTime < minimumLoadTime)
                {
                    float waitTime = minimumLoadTime - loadElapsedTime;
                    DebugLog($"Waiting {waitTime:F2}s to meet minimum load time");
                    yield return new WaitForSeconds(waitTime);
                }
                
                // Show 100%
                if (progressBar != null)
                    progressBar.value = 1f;
                if (progressText != null)
                    progressText.text = "100%";
                
                DebugLog("Showing 100% and waiting 0.3s before activation");
                yield return new WaitForSeconds(0.3f);
                
                // Activate the scene
                DebugLog("Activating scene...");
                operation.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        DebugLog("=== LOADING COMPLETE ===");
    }
    
    private void DebugLog(string message)
    {
        if (enableDebugLogging)
        {
            Debug.Log($"[MainMenu] {message}");
        }
    }
    
    private void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        
        if (quitButton != null)
            quitButton.onClick.RemoveListener(OnQuitButtonClicked);
    }
}