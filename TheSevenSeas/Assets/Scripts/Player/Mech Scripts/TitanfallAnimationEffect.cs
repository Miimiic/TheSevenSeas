using UnityEngine;

public class TitanfallAnimationEffect : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [Header("Animated Game Objects")]
    [SerializeField] private GameObject centreRing;
    [SerializeField] private GameObject midRing;
    [SerializeField] private GameObject outerRing;
    [SerializeField] private GameObject column;

    [Header("Animation Variables")]
    [SerializeField] private float animSpeed;
    [SerializeField] private float animationLifetime = 3;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        animationLifetime -= Time.deltaTime;
        if (animationLifetime < 0)
        {
            Destroy(gameObject);
        }
        HandleAnimation();
    }

    void HandleAnimation()
    {
        centreRing.transform.Rotate(0,animSpeed,0);
        midRing.transform.Rotate(0,-animSpeed,0);
        outerRing.transform.Rotate(0, animSpeed,0);
        column.transform.Rotate(0,-animSpeed,0);
    }
}
