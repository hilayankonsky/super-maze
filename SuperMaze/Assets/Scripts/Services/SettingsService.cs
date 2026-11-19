public class SettingsService : ISettingsService
{
    const string VibrationKey = "settings_vibration_on";
    const string SoundKey = "settings_sound_on";

    readonly ISaveService save;

    public SettingsService(ISaveService saveService)
    {
        save = saveService;
    }

    public bool VibrationOn
    {
        get => save.GetInt(VibrationKey, 1) != 0;
        set => save.SetInt(VibrationKey, value ? 1 : 0);
    }

    public bool SoundOn
    {
        get => save.GetInt(SoundKey, 1) != 0;
        set => save.SetInt(SoundKey, value ? 1 : 0);
    }
}
