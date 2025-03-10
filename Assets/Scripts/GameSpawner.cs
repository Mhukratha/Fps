using UnityEngine;
using Unity.Netcode;

public class GameSpawner : NetworkBehaviour
{
    public GameObject playerPrefab;
    public Transform playerSpawnPoint; // ✅ ตำแหน่ง Spawn ที่กำหนด

    public override void OnNetworkSpawn()
    {
        if (IsServer) 
        {
            // ✅ Spawn Host ทันทีที่เริ่มเกม
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);

            // ✅ Spawn Client เมื่อเชื่อมต่อ
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnPlayer;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;

        Debug.Log($"🎮 Client {clientId} เชื่อมต่อ -> Spawn Player");

        // ✅ ใช้ตำแหน่งจาก SpawnPoint
        Vector3 spawnPosition = playerSpawnPoint != null ? playerSpawnPoint.position : new Vector3(0, 1, 0);

        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        
        if (playerInstance == null)
        {
            Debug.LogError("❌ Player Spawn ไม่สำเร็จ!");
        }
        else
        {
            Debug.Log("✅ Player Spawn สำเร็จที่ " + spawnPosition);
            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }
}
