using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);       
        Services.Save = new PlayerPrefsSaveService();
        Services.Settings = new SettingsService(Services.Save);
        Debug.Log("Bootstrap: services ready");
        SceneManager.LoadScene("Home");     
    }
}
