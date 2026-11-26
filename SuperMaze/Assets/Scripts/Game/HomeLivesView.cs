using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_Text))]
public class HomeLivesView : MonoBehaviour
{
    [SerializeField] TMP_Text limitLabel;

    TMP_Text label;
    ISaveService subscribedSave;
    float nextRefresh;

    void OnEnable()
    {
        label = GetComponent<TMP_Text>();
        subscribedSave = Services.Save;
        if (subscribedSave != null)
        {
            subscribedSave.OnValueChanged += SavedValueChanged;
        }

        if (limitLabel != null && Services.Lives != null)
        {
            limitLabel.text = $"/{Services.Lives.MaxLives}";
        }

        Refresh();
    }

    void OnDisable()
    {
        if (subscribedSave != null)
        {
            subscribedSave.OnValueChanged -= SavedValueChanged;
            subscribedSave = null;
        }
    }

    void Update()
    {
        if (Time.unscaledTime >= nextRefresh)
        {
            nextRefresh = Time.unscaledTime + 1f;
            Refresh();
        }
    }

    void SavedValueChanged(string key) => Refresh();

    void Refresh()
    {
        var lives = Services.Lives;
        if (lives != null)
        {
            label.text = lives.CurrentLives.ToString();
        }
    }
}
