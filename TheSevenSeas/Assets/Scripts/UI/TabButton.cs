using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TabButtonScript : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TabGroup tabGroup;
    public Image backgroundImage;
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEntered(this);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }
    
    void Start()
    {
        backgroundImage = GetComponent<Image>();
        tabGroup.Subscribe(this);
        tabGroup.OnTabEntered(this);
    }
}