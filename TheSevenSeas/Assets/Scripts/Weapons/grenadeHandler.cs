using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class grenadeHandler : MonoBehaviour
{
    [Header("References")]
    public Transform cam;
    public Transform attackPT;
    public GameObject NailBomb;
    public TextMeshProUGUI text;

    [Header("Settings")]
    public int GrenadesLeft;
    public KeyCode throwKey;
    public float cooldownOnThrow;
    public float throwForce;
    public float throwHeight;


    bool readyTothrow;


    private void Start()
    {
        readyTothrow = true;
    }
    private void Update()
    {
        if (text == null)
        {
            text = GameObject.FindGameObjectWithTag("GrenadeCounter").GetComponent<TextMeshProUGUI>();

        }
        if (Input.GetKeyDown(throwKey) && readyTothrow && GrenadesLeft>0)
        {
            Throw();
            
        }
        text.SetText(GrenadesLeft.ToString());
    }
    void Throw() 
    {

        GameObject grenade = Instantiate(NailBomb, attackPT.position, cam.rotation);
        Rigidbody grenadeRB = grenade.GetComponent<Rigidbody>();
        Vector3 Force = cam.transform.forward * throwForce + transform.up * throwHeight;
        grenadeRB.AddForce(Force,ForceMode.Impulse);
        GrenadesLeft--;
        readyTothrow = false;
        Invoke("cooldown", cooldownOnThrow);
    
    }
    void cooldown() 
    {

        readyTothrow = true;
        
    
    }
}
