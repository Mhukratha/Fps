using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using System.Collections;

public class LoadingSceneManager : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(LoadMultiplayerScene());
    }

    IEnumerator LoadMultiplayerScene()
    {
        yield return new WaitForSeconds(3); // จำลองการโหลด 3 วินาที

        if (PlayerPrefs.GetInt("IsHost") == 1)
        {
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }

        SceneManager.LoadScene("Multiplayer");
    }
}
