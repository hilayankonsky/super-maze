
using TMPro;
using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    [SerializeField] TMP_Text label;

    private void OnEnable()
    {
        Services.Save.OnValueChanged += HandleValueChanged;
        Show();
    }

    private void OnDisable()
    {
        Services.Save.OnValueChanged -= HandleValueChanged;
    }

    private void HandleValueChanged(string key)
    {
        if (key == "coins")
        {
            Show();
        }
    }

    public void Add(int amount)
    {
        Services.Save.SetInt(
            "coins",
            Services.Save.GetInt("coins") + amount
        );
    }

    private void Show()
    {
        label.text = Services.Save.GetInt("coins").ToString();
    }
}
