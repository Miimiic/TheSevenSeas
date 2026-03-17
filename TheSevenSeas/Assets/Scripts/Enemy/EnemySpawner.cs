using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemySpawner : MonoBehaviour
{
    [Header("General")]
    public GameObject enemyPrefab;
    public Transform player;

    [Header("Spawn Settings")]
    public float maxSpawnDist;
    public float minSpawnDist;
    public int maxEnemies;
    public float spawnInterval;

    [Header("Despawn Settings")]
    public float despawnDist;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        if (player == null)
        {
            // Grab the top-level Player object, not a child
            GameObject playerObj = GameObject.FindWithTag("Player");
            player = playerObj.transform.root; // forces it to the root of the hierarchy 
        }
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Clean up destroyed enemies
            spawnedEnemies.RemoveAll(e => e == null);

            // Despawn enemies that are too far away
            for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
            {
                GameObject enemy = spawnedEnemies[i];
                if (Vector3.Distance(enemy.transform.position, player.position) > despawnDist)
                {
                    Destroy(enemy);
                    spawnedEnemies.RemoveAt(i);
                    // Don't continue here — let the spawn check below refill the slot
                }
            }

            // Spawn up to maxEnemies
            if (spawnedEnemies.Count >= maxEnemies)
                continue;

            Vector3 spawnPosition = GetRandomSpawnPosition();
            if (spawnPosition != Vector3.zero)
            {
                GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                spawnedEnemies.Add(enemy);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPos = player.position + Random.insideUnitSphere * maxSpawnDist;
            randomPos.y = player.position.y;

            float distanceToPlayer = Vector3.Distance(randomPos, player.position);
            if (distanceToPlayer < minSpawnDist)
                continue;

            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                if (Vector3.Distance(hit.position, player.position) > minSpawnDist)
                    return hit.position;
            }
        }
        return Vector3.zero;
    }
}
