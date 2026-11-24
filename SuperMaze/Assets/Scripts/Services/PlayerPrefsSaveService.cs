using System;
using UnityEngine;

public class PlayerPrefsSaveService : ISaveService
{
    public event Action<string> OnValueChanged;

    public int GetInt(string key, int fallback = 0)
    {
        return PlayerPrefs.GetInt(key, fallback);
    }

    public void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();

        OnValueChanged?.Invoke(key);
    }
}