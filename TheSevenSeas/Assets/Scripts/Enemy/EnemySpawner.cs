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
    public float spawnRadius;          
    public float minSpawnDistance;     
    public int maxEnemies;
    public float spawnInterval;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void Start()
    {
        if (player == null)
            player = GameObject.FindWithTag("Player").transform;

        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Check max enemies
            spawnedEnemies.RemoveAll(e => e == null);
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
        for (int i = 0; i < maxEnemies; i++) 
        {
            Vector3 randomPos = player.position + Random.insideUnitSphere * spawnRadius;
            randomPos.y = player.position.y;

            float distanceToPlayer = Vector3.Distance(randomPos, player.position);
            if (distanceToPlayer < minSpawnDistance)
                continue;

            // Check NavMesh
            if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                // Optional: check distance so enemy doesn’t spawn too close
                if (Vector3.Distance(hit.position, player.position) > 10f)
                    return hit.position;
            }
        }

        return Vector3.zero; // failed to find a position
    }

}
