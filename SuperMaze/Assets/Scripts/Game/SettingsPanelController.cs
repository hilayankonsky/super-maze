using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelController : MonoBehaviour
{
    [SerializeField] Button openButton;
    [SerializeField] GameObject panelRoot;
    [SerializeField] Toggle vibrationToggle;
    [SerializeField] Button closeButton;

    void Awake()
    {
        openButton.onClick.AddListener(Open);
        closeButton.onClick.AddListener(Close);
        vibrationToggle.onValueChanged.AddListener(SetVibration);
        panelRoot.SetActive(false);
    }

    void OnDestroy()
    {
        if (openButton != null)
        {
            openButton.onClick.RemoveListener(Open);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
        }

        if (vibrationToggle != null)
        {
            vibrationToggle.onValueChanged.RemoveListener(SetVibration);
        }
    }

    void Open()
    {
        vibrationToggle.SetIsOnWithoutNotify(Services.Settings.VibrationOn);
        panelRoot.SetActive(true);
        panelRoot.transform.SetAsLastSibling();
    }

    void Close() => panelRoot.SetActive(false);

    void SetVibration(bool enabled) => Services.Settings.VibrationOn = enabled;
}
