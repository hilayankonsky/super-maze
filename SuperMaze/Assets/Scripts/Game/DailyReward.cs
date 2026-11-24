
using System;
using TMPro;
using UnityEngine;

// DailyReward.cs (Assets/Scripts/Game): on the Daily gift button in Home
public class DailyReward : MonoBehaviour
{
    [SerializeField] CoinCounter coins;
    [SerializeField] TMP_Text label;

    const string Key = "daily_last_day";

    int Today => (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalDays;
    bool ClaimedToday => Services.Save.GetInt(Key, -1) == Today;

    void Start() => Show();

    void Show()
    {
        label.text = ClaimedToday
            ? "Come back tomorrow"
            : $"Daily gift: +{Services.Config.Economy.dailyReward}";
    }

    public void Claim()
    {
        if (ClaimedToday) return;

        Services.Save.SetInt(Key, Today);
        coins.Add(Services.Config.Economy.dailyReward);
        Show();
    }

    [ContextMenu("Reset daily")]
    void ResetDaily()
    {
        Services.Save.SetInt(Key, -1);
        Show();
    }
}
