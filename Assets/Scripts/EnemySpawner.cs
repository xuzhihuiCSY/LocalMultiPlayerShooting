using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Spawning")]
    public GameObject[] enemyPrefabs; // Drag multiple enemy prefabs into this array in Inspector
    public float spawnInterval = 5f;
    public int maxEnemies = 3;

    private float timer = 0f;

    void Update()
    {
        if (!IsServer) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            timer = 0f;
            TrySpawnEnemy();
        }
    }

    private void TrySpawnEnemy()
    {
        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentEnemyCount >= maxEnemies) return;

        Vector3 spawnPos = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));

        // 🔀 Choose a random enemy prefab from the array
        GameObject chosenPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

        GameObject enemy = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);

        NetworkObject netObj = enemy.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }
}
