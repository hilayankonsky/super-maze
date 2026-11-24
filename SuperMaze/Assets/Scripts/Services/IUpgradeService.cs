public interface IUpgradeService
{
    int GetLevel(string upgradeId);
    int GetPrice(string upgradeId);
    bool CanUpgrade(string upgradeId);
    bool TryUpgrade(string upgradeId);
}