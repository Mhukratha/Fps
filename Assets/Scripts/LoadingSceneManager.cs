using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class LoadingSceneManager : MonoBehaviour
{
    private bool isHost;

    void Start()
    {
        // เช็คว่า NetworkManager ทำงานอยู่แล้วหรือไม่
        if (NetworkManager.Singleton.IsListening)
        {
            Debug.LogWarning("NetworkManager is already running. Skipping initialization.");
            return;
        }

        isHost = PlayerPrefs.GetInt("IsHost") == 1;

        if (isHost)
        {
            Debug.Log("Starting Host...");
            if (!NetworkManager.Singleton.StartHost()) // ตรวจสอบว่า StartHost() สำเร็จหรือไม่
            {
                Debug.LogError("Failed to start Host. Port might be in use. Shutting down.");
                return;
            }

            StartCoroutine(WaitForClientsAndLoadScene());
        }
        else
        {
            Debug.Log("Starting Client...");
            if (!NetworkManager.Singleton.StartClient()) // ตรวจสอบว่า StartClient() สำเร็จหรือไม่
            {
                Debug.LogError("Failed to start Client. Shutting down.");
                return;
            }
        }
    }

    IEnumerator WaitForClientsAndLoadScene()
    {
        Debug.Log("Waiting for clients to join...");

        while (NetworkManager.Singleton.ConnectedClientsList.Count < 2) // รอจนกว่ามี Client อย่างน้อย 1 คน
        {
            yield return new WaitForSeconds(1);
        }

        Debug.Log("Client joined! Loading Multiplayer Scene...");
        SceneManager.LoadScene("Mul"); // เปลี่ยนไป Multiplayer Scene
    }
}
