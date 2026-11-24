using System;

public interface ISaveService
{
    event Action<string> OnValueChanged;

    int GetInt(string key, int fallback = 0);
    void SetInt(string key, int value);
}