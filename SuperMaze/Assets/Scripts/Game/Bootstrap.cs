using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    [SerializeField] private EconomyConfig economyConfig;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        Services.Save = new PlayerPrefsSaveService();
        Services.Config = new ConfigService(economyConfig);
        Services.Settings = new SettingsService(Services.Save);

        Services.Upgrades = new UpgradeService(
            Services.Save,
            Services.Config.Economy
        );

        Services.Boosts = new BoostService(
            Services.Save,
            Services.Config.Economy
        );

        Debug.Log("Bootstrap: services ready");

        SceneManager.LoadScene("Home");
    }
}