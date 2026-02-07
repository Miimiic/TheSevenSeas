using UnityEngine;
using UnityEngine.UI;

public class BuildingButton : MonoBehaviour
{
    [Header("References")]
    public GameObject prefabToPlace; 
    public BuildingModeManager buildingManager; 
    
    private Button button;
    
    void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }
    
    void OnButtonClick()
    {
        buildingManager.SelectPrefab(prefabToPlace);
    }
}