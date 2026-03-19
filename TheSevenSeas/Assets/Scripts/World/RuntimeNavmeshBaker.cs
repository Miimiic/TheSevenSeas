using System.Collections;
using UnityEngine;
using Unity.AI.Navigation;

public class RuntimeNavMeshBaker : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;
    [SerializeField] private float rebakeDelay = 0.5f; // slight delay if needed

    private Coroutine rebakeCoroutine;

    private void Awake()
    {
        // Auto-grab it if not assigned in Inspector
        if (navMeshSurface == null)
            navMeshSurface = GetComponent<NavMeshSurface>();
    }

    private void Start()
    {
        BakeNavMesh(); // initial bake on scene load
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            RequestRebake();
        }
    }


    public void RequestRebake()
    {
        if (rebakeCoroutine != null)
            StopCoroutine(rebakeCoroutine);

        rebakeCoroutine = StartCoroutine(RebakeAfterDelay());
    }

    private IEnumerator RebakeAfterDelay()
    {
        yield return new WaitForSeconds(rebakeDelay);
        BakeNavMesh();
    }

    private void BakeNavMesh()
    {
        navMeshSurface.BuildNavMesh();
        Debug.Log("NavMesh baked at runtime.");
    }
}
