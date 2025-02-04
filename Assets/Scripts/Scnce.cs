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
}
