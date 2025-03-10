using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class Scnce : MonoBehaviour
{
       public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("🖥️ Host Started");
        SceneManager.LoadScene("Loading"); // ✅ เปลี่ยนไปที่ Scene "Loading"
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("🎮 Client Started");
        SceneManager.LoadScene("Loading"); // ✅ เปลี่ยนไปที่ Scene "Loading"
    }
}
