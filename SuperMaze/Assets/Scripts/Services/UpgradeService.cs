
using System;

public class UpgradeService : IUpgradeService
{
    private readonly ISaveService save;
    private readonly EconomyConfig economy;

    public UpgradeService(ISaveService save, EconomyConfig economy)
    {
        this.save = save;
        this.economy = economy;
    }

    private string GetKey(string upgradeId)
    {
        if (upgradeId != "shoes" && upgradeId != "flashlight")
            throw new ArgumentException("Unknown upgrade ID");

        return $"upgrade_{upgradeId}_level";
    }

    public int GetLevel(string upgradeId)
    {
        return save.GetInt(GetKey(upgradeId), 1);
    }

    public int GetPrice(string upgradeId)
    {
        int level = GetLevel(upgradeId);

        if (level >= economy.maxUpgradeLevel)
            return 0;

        return economy.GetUpgradePrice(level);
    }

    public bool CanUpgrade(string upgradeId)
    {
        if (GetLevel(upgradeId) >= economy.maxUpgradeLevel)
            return false;

        int balance = save.GetInt("coins");

        return balance >= GetPrice(upgradeId);
    }

    public bool TryUpgrade(string upgradeId)
    {
        if (!CanUpgrade(upgradeId))
            return false;

        int price = GetPrice(upgradeId);
        int balance = save.GetInt("coins");
        int level = GetLevel(upgradeId);

        save.SetInt("coins", balance - price);
        save.SetInt(GetKey(upgradeId), level + 1);

        return true;
    }
}
