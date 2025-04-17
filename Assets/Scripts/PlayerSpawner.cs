using Unity.Netcode;
using UnityEngine;
using System.Collections;
public class PlayerSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Spawn a player for all already connected clients
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayer(client.ClientId);
        }

        // If new client connects after the game started
        NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
    }

    public void SpawnPlayer(ulong clientId)
    {
        Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"✅ Spawned player for client {clientId}");
    }
    public void ScheduleRespawn(ulong clientId, float delay)
    {
        Debug.Log($"[PlayerSpawner] Will schedule respawn in {delay} seconds for client {clientId}");
        StartCoroutine(RespawnAfterDelay(clientId, delay));
    }

    private IEnumerator RespawnAfterDelay(ulong clientId, float delay)
    {
        yield return new WaitForSeconds(delay);
        Debug.Log($"[PlayerSpawner] Time's up. Now respawning client {clientId}");
        RespawnPlayer(clientId);
    }
    // 🔁 This is a new method for respawning after death
    public void RespawnPlayer(ulong clientId)
    {
        Debug.Log($"[PlayerSpawner] RespawnPlayer called for client {clientId}");

        Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        Debug.Log($"[PlayerSpawner] Instantiated new player prefab at {spawnPos}. Now spawning as player object...");

        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        Debug.Log($"♻ [PlayerSpawner] Successfully respawned player for client {clientId}");
    }

}
