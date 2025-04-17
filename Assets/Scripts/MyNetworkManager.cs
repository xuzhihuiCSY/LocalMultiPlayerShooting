using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class MyNetworkManager : NetworkManager
{
    // Assign your player prefab here in the Inspector
    public GameObject playerPrefab;

    public static MyNetworkManager Instance;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Auto-fill if not manually assigned
        if (playerPrefab == null)
        {
            playerPrefab = NetworkConfig.PlayerPrefab;
            Debug.Log("Auto-assigned playerPrefab from NetworkConfig.");
        }

        OnServerStarted += HandleServerStarted;
    }


    private void HandleServerStarted()
    {
        Debug.Log("[MyNetworkManager] Server has started");
    }

    public void ScheduleRespawn(ulong clientId, float delay)
    {
        Debug.Log($"[MyNetworkManager] Scheduling respawn for client {clientId} in {delay}s");
        StartCoroutine(RespawnAfterDelay(clientId, delay));
    }

    private IEnumerator RespawnAfterDelay(ulong clientId, float delay)
    {
        yield return new WaitForSeconds(delay);
        RespawnPlayer(clientId);
    }

    public void RespawnPlayer(ulong clientId)
    {
        Debug.Log($"[MyNetworkManager] RespawnPlayer called for client {clientId}");

        Vector3 spawnPos = new Vector3(Random.Range(-5f, 5f), 1f, Random.Range(-5f, 5f));
        GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);

        NetworkObject netObj = playerObj.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId);

        Debug.Log($"[MyNetworkManager] Player for client {clientId} spawned at {spawnPos}");
    }
}
