using UnityEngine;

public class deactivateOverTime : MonoBehaviour
{
    public float timeTillFalse;

    private void OnEnable()
    {
        Invoke("deactivate", timeTillFalse);
    }
    void deactivate() 
    { 
    
     gameObject.SetActive(false);
    
    }
   
}
