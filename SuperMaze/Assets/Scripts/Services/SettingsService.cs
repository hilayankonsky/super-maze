public class SettingsService : ISettingsService
{
    const string VibrationKey = "settings_vibration_on";

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
}
