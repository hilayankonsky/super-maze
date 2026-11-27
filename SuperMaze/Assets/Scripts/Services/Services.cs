using UnityEngine;

// Services.cs: where every script finds the services
public static class Services
{
    public static ISaveService Save { get; set; }
    public static ISettingsService Settings { get; set; }
    public static IConfigService Config { get; set; }
    public static IUpgradeService Upgrades { get; set; }
    public static IBoostService Boosts { get; set; }
    public static ILifeService Lives { get; set; }
}

