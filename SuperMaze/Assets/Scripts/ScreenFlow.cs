using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenFlow : MonoBehaviour
{
     public void Play()                     // Play button → On Click () → ScreenFlow.Play
    {
        SceneManager.LoadScene("Level");   // the exact scene name
    }
    public void Back()                     // Back button → On Click () → ScreenFlow.Back
    {
        SceneManager.LoadScene("Home");
    }
}
