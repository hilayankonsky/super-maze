using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);       
        Services.Save = new PlayerPrefsSaveService(); 
        Debug.Log("Bootstrap: services ready");
        SceneManager.LoadScene("Home");     
    }
}
