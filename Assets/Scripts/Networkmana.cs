using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

public class NetworkManagerScript : MonoBehaviour
{
     private void Start()
    {
        if (!NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
        {
            StartHost(); 
            StartClient();
        }
        
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("Host started");
    }

    // Cli
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Client started");
    }

    //Serv
    public void StartServer()
    {
        NetworkManager.Singleton.StartServer();
        Debug.Log("Server started");
    }
}