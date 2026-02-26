using UnityEngine;

public class BuildingModeManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buildingUI;
    
    [Header("Grid System")]
    public GridSystem gridSystem;
    
    [Header("Camera")]
    public PlayerController playerController;
    
    [Header("Player Controls (Optional)")]
    public MonoBehaviour[] playerScriptsToDisable;
    
    private bool isBuildingModeActive = false;
    
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
        // Toggle building menu with 'M' key (only when in build mode)
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (playerController != null && playerController.IsBuildMode())
            {
                ToggleBuildingMenu();
            }
            else
            {
                Debug.Log("Switch to Build Mode (press B) to access building menu");
            }
        }
        
        // When player enters build mode (B pressed)
        if (playerController != null && playerController.IsBuildMode())
        {
            // If grid was disabled and menu is not open, enable grid
            if (gridSystem != null && !gridSystem.isActiveAndEnabled && !isBuildingModeActive)
            {
                gridSystem.SetEnabled(true);
            }
        }
        else
        {
            // If player exits build mode, close the UI and grid
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
        
        // When menu opens, show cursor and disable grid
        // When menu closes, hide cursor and enable grid
        if (isBuildingModeActive)
        {
            // Show cursor for UI interaction
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            
            // Disable grid while browsing menu
            if (gridSystem != null)
            {
                gridSystem.SetEnabled(false);
            }
            
            SetPlayerControlsEnabled(false);
        }
        else
        {
            // Hide cursor and re-enable grid for placement
            RestoreCursorForGameplay();
            
            if (gridSystem != null && playerController != null && playerController.IsBuildMode())
            {
                gridSystem.SetEnabled(true);
            }
            
            SetPlayerControlsEnabled(true);
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
        
        // Close the UI
        buildingUI.SetActive(false);
        isBuildingModeActive = false;
        
        // Restore cursor for gameplay (locked)
        RestoreCursorForGameplay();
        
        // Enable grid system if we're in build mode
        if (gridSystem != null && playerController != null && playerController.IsBuildMode())
        {
            gridSystem.SetEnabled(true);
        }
        
        // Re-enable player controls
        SetPlayerControlsEnabled(true);
    }
    
    void SetPlayerControlsEnabled(bool enabled)
    {
        foreach (var script in playerScriptsToDisable)
        {
            if (script != null)
            {
                script.enabled = enabled;
            }
        }
    }
}