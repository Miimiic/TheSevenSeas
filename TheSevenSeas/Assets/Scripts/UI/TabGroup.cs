using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TabGroup : MonoBehaviour
{
    public List<TabButtonScript> tabButtons;
    public Color tabIdle = new Color(20f,20f,20f,1f);
    public Color tabHover = new Color(25f,25f,25f,1f);
    public Color tabActive = new Color(30f,30f,30f,1f);
    public TabButtonScript selectedTab;
    public List<GameObject> objectsToSwap;
    
    public void Subscribe(TabButtonScript button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButtonScript>();
        }
        tabButtons.Add(button);
    }
    
    public void OnTabEntered(TabButtonScript button)
    {
        if(selectedTab == null || button != selectedTab)
        {
            ResetTabs();
            button.backgroundImage.color = tabHover;
        }
    }
    
    public void OnTabExit(TabButtonScript button)
    {
        ResetTabs();
    }
    
    public void OnTabSelected(TabButtonScript button)
    {
        selectedTab = button;
        ResetTabs();
        button.backgroundImage.color = tabActive;
        
        int index = button.transform.GetSiblingIndex();
        for (int i = 0; i < objectsToSwap.Count; i++)
        {
            if (i == index)
            {
                objectsToSwap[i].SetActive(true);
            }
            else
            {
                objectsToSwap[i].SetActive(false);
            }
        }
    }
    
    public void ResetTabs()
    {
        foreach (TabButtonScript button in tabButtons)
        {
            if (selectedTab != null && button == selectedTab)
            {
                button.backgroundImage.color = tabActive;
                continue;
            }
            button.backgroundImage.color = tabIdle;
        }
    }
}