
using UnityEngine;

[CreateAssetMenu(
    fileName = "EconomyConfig",
    menuName = "SuperMaze/Economy Config")]
public class EconomyConfig : ScriptableObject
{
    [Header("Level Settings")]
    public float levelDuration = 120f;

    [Header("Level Rewards")]
    public int oneStarReward = 40;
    public int twoStarReward = 80;
    public int threeStarReward = 120;

    [Header("Star Time Thresholds")]
    public float threeStarTime = 40f;
    public float twoStarTime = 80f;

    [Header("Daily Reward")]
    public int dailyReward = 50;

    [Header("Boost Prices")]
    public int extraLifePrice = 100;
    public int extraTimePrice = 120;
    public int defenseBubblePrice = 150;
    public int removeItemPrice = 180;

    [Header("Boost Effects")]
    public int extraTimeSeconds = 10;

    [Header("Upgrade Pricing")]
    public int upgradeBasePrice = 50;
    public float upgradeGrowthFactor = 1.4f;
    public int maxUpgradeLevel = 5;

    [Header("Upgrade Effects")]
    public float shoesSpeedIncrease = 0.05f;
    public float flashlightRangeIncrease = 0.10f;

    public int GetUpgradePrice(int currentLevel)
    {
        return Mathf.RoundToInt(
            upgradeBasePrice *
            Mathf.Pow(upgradeGrowthFactor, currentLevel - 1)
        );
    }
}
