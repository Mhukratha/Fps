using UnityEngine;
using UnityEngine.SceneManagement; 


public class Scnce : MonoBehaviour
{
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
}

