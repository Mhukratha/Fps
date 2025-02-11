using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using Unity.Netcode.Transports.UTP;
using UnityEngine.SceneManagement; 

public class NetworkManagerScript : MonoBehaviour
{
      void Start()
    {
        if (SystemInfo.deviceName.Contains("Player1"))
        {
            StartServer();
        }
    }

    private void StartServer()
    {
        Debug.Log("Starting Server...");
        NetworkManager.Singleton.StartServer();
    }

}