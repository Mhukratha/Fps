using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class LoadingManager : MonoBehaviour
{
    private void Start()
    {
        // ถ้าเป็น Host ให้เช็คว่ามีผู้เล่นครบ 2 คนแล้วหรือยัง
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += CheckPlayers;
        }
    }

    private void CheckPlayers(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= 2)
        {
            SceneManager.LoadScene("Mul"); // เมื่อมีผู้เล่นครบ 2 คน ให้เข้าเกม
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= CheckPlayers;
        }
    }
}
