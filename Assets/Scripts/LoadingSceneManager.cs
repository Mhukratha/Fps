using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class LoadingSceneManager : MonoBehaviour
{
    private bool isHost;

    void Start()
    {
        isHost = PlayerPrefs.GetInt("IsHost") == 1;

        if (isHost)
        {
            NetworkManager.Singleton.StartHost();
            StartCoroutine(WaitForClientsAndLoadScene());
        }
        else
        {
            NetworkManager.Singleton.StartClient();
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
