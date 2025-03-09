using UnityEngine;
using UnityEngine.SceneManagement; 


public class Scnce : MonoBehaviour
{
    private static bool isHostCreated = false;

     public void Multi()
    {
        SceneManager.LoadScene("Mul");
    }

     public void Solo()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void Exit()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
 
     public void JoinGame()
    {
        if (!isHostCreated)
        {
            isHostCreated = true;
            PlayerPrefs.SetInt("IsHost", 1);
        }
        else
        {
            PlayerPrefs.SetInt("IsHost", 0);
        }

        SceneManager.LoadScene("Loading");
    }
}

