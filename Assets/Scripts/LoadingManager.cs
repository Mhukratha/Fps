using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class LoadingManager : MonoBehaviour
{
    private int requiredPlayers = 2;

    private void Start()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += CheckPlayers;
        }
    }

    private void CheckPlayers(ulong clientId)
    {
        if (NetworkManager.Singleton == null) return; // ✅ ป้องกัน NullReferenceException

        int currentPlayers = NetworkManager.Singleton.ConnectedClientsList.Count;
        Debug.Log($"✅ Client {clientId} joined. จำนวนผู้เล่นตอนนี้: {currentPlayers}/{requiredPlayers}");

        if (currentPlayers >= requiredPlayers)
        {
            Debug.Log("🎮 ผู้เล่นครบแล้ว! เริ่มเกม...");
            StartGame();
        }
    }

    private void StartGame()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("Mul", LoadSceneMode.Single);
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= CheckPlayers;
        }
    }
}
