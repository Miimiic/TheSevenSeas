using UnityEngine;

public class grenadeHandler : MonoBehaviour
{
    public GameObject NailBomb;
    [SerializeField]private int grenadesLeft;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) 
        {
            if (grenadesLeft > 0) 
            {

                Debug.Log("Grenade Will be thrown");
                grenadesLeft -= 1;
                Instantiate(NailBomb,gameObject.transform);
            
            }
            if (grenadesLeft <= 0)
            {

                Debug.Log("No grenades Left");

            }


        }
    }
}
