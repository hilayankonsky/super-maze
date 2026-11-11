
using UnityEngine;
using UnityEngine.SceneManagement;

public class HomeButtonHandler : MonoBehaviour
{
    public void GoHome()
    {
        SceneManager.LoadScene("Home");
    }
}
