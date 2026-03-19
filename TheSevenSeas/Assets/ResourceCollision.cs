using UnityEngine;

public class ResourceCollision : MonoBehaviour
{


    PlayerInventoryController invCont;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        invCont = PlayerInventoryController.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.tag=="Player")
        {
            int material = Mathf.RoundToInt(Random.Range(0, 7));
            Debug.Log(material);
            switch (material)
            {
                case 0:
                    PlayerInventoryController.Instance.AddWood(10);
                    break;
                case 1:
                    PlayerInventoryController.Instance.AddBrick(10);
                    break;
                case 2:
                    PlayerInventoryController.Instance.AddMetal(10);
                    break;
                case 3:
                    PlayerInventoryController.Instance.AddNail(10);
                    break;
                case 4:
                    PlayerInventoryController.Instance.AddMetalPipe(10);
                    break;
                case 5:
                    PlayerInventoryController.Instance.AddWorkerFlesh(10);
                    break;
                case 6:
                    PlayerInventoryController.Instance.AddInstructionManual(10);
                    break;
                case 7:
                    PlayerInventoryController.Instance.AddEldritchHeart(10);
                    break;
            }
            Destroy(gameObject);
        }
    }

}
