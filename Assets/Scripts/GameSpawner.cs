using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class GameSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;
    public Transform playerSpawnPoint;

    private Dictionary<ulong, GameObject> spawnedPlayers = new Dictionary<ulong, GameObject>();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // ✅ Spawn ตัวเอง (Host)
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);

            // ✅ Spawn Client ที่เชื่อมต่อ
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer || spawnedPlayers.ContainsKey(clientId)) return;

        Debug.Log($"🎮 Client {clientId} เชื่อมต่อ -> Spawn Player");

        Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(0, 1, 0);

        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        spawnedPlayers.Add(clientId, playerInstance);
    }
}
