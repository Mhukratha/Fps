using UnityEngine;
using UnityEngine.SceneManagement;

public class UImanager : MonoBehaviour
{
   public void RestartGame()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    public void GoToMenu()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene("Menu"); 
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}

