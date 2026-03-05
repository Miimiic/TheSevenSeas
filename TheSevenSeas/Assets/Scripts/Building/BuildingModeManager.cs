using UnityEngine;
public class BuildingModeManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buildingUI;
    
    [Header("Grid System")]
    public GridSystem gridSystem;
    
    [Header("Camera")]
    public PlayerController playerController;
    
    [Header("Player Controls")]
    [Tooltip("Disabled while menu is open, re-enabled when menu closes (e.g. movement, look)")]
    public MonoBehaviour[] menuOnlyDisabledScripts;

    [Tooltip("Disabled when entering build mode, only re-enabled when exiting build mode (e.g. gun, shooting)")]
    public MonoBehaviour[] buildModeDisabledScripts;
    
    private bool isBuildingModeActive = false;
    private bool wasInBuildModeLastFrame = false;
    
    void Start()
    {
        buildingUI.SetActive(false);
        
        if (gridSystem != null)
        {
            gridSystem.SetEnabled(false);
        }
    }
    
    void Update()
    {
        bool isInBuildMode = playerController != null && playerController.IsBuildMode();

        // Detect when player enters build mode
        if (!wasInBuildModeLastFrame && isInBuildMode)
        {
            SetScriptsEnabled(buildModeDisabledScripts, false);
        }

        // Detect when player exits build mode (B pressed to leave)
        if (wasInBuildModeLastFrame && !isInBuildMode)
        {
            SetScriptsEnabled(buildModeDisabledScripts, true);
            SetScriptsEnabled(menuOnlyDisabledScripts, true);
        }
        wasInBuildModeLastFrame = isInBuildMode;

        // Toggle building menu with 'M' key (only when in build mode)
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (isInBuildMode)
            {
                ToggleBuildingMenu();
            }
            else
            {
                Debug.Log("Switch to Build Mode (press B) to access building menu");
            }
        }
        
        if (isInBuildMode)
        {
            if (gridSystem != null && !gridSystem.isActiveAndEnabled && !isBuildingModeActive)
            {
                gridSystem.SetEnabled(true);
            }
        }
        else
        {
            if (isBuildingModeActive)
            {
                buildingUI.SetActive(false);
                isBuildingModeActive = false;
                RestoreCursorForGameplay();
            }
            
            if (gridSystem != null && gridSystem.isActiveAndEnabled)
            {
                gridSystem.SetEnabled(false);
            }
        }
    }
    
    void ToggleBuildingMenu()
    {
        isBuildingModeActive = !isBuildingModeActive;
        buildingUI.SetActive(isBuildingModeActive);
        
        if (isBuildingModeActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            if (gridSystem != null)
                gridSystem.SetEnabled(false);
            
            // Disable movement/look while menu is open
            SetScriptsEnabled(menuOnlyDisabledScripts, false);
        }
        else
        {
            RestoreCursorForGameplay();
            
            if (gridSystem != null && playerController != null && playerController.IsBuildMode())
                gridSystem.SetEnabled(true);
            
            // Re-enable movement/look when menu closes (still in build mode)
            SetScriptsEnabled(menuOnlyDisabledScripts, true);
            // buildModeDisabledScripts stay OFF until player exits build mode
        }
    }
    
    void RestoreCursorForGameplay()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    public void SelectPrefab(GameObject prefab)
    {
        if (gridSystem != null)
        {
            gridSystem.objectToPlace = prefab;
            gridSystem.CreateGhostObject();
        }
        
        buildingUI.SetActive(false);
        isBuildingModeActive = false;
        
        RestoreCursorForGameplay();
        
        if (gridSystem != null && playerController != null && playerController.IsBuildMode())
            gridSystem.SetEnabled(true);
        
        // Re-enable movement/look after selecting (still in build mode)
        SetScriptsEnabled(menuOnlyDisabledScripts, true);
        // buildModeDisabledScripts stay OFF until player exits build mode
    }
    
    void SetScriptsEnabled(MonoBehaviour[] scripts, bool enabled)
    {
        foreach (var script in scripts)
        {
            if (script != null)
                script.enabled = enabled;
        }
    }
}