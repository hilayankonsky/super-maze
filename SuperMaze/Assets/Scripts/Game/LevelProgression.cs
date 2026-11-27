using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LevelProgression
{
    static string[] attemptBoosts = Array.Empty<string>();

    public static int SelectedLevel { get; private set; } = 1;

    private static int? pendingPopupLevel;

    public static void RequestLevelPopup(int level)
    {
        pendingPopupLevel = level;
    }

    public static int? TakePendingPopupLevel()
    {
        int? level = pendingPopupLevel;
        pendingPopupLevel = null;
        return level;
    }

    public static int BestStars(int level) => Mathf.Clamp(Services.Save?.GetInt(BestStarsKey(level)) ?? 0, 0, 3);

    public static bool IsUnlocked(int level) => level > 0 &&
        (level == 1 || BestStars(level) > 0 || BestStars(level - 1) > 0);

    public static bool TrySelect(int level, IEnumerable<string> selectedBoosts = null)
    {
        if (!IsUnlocked(level) || Services.Lives == null || Services.Lives.CurrentLives == 0)
        {
            return false;
        }

        var boosts = selectedBoosts?.Distinct().ToArray() ?? Array.Empty<string>();
        var inventory = Services.Boosts;
        if (boosts.Any(id => inventory == null || inventory.GetQuantity(id) < 1))
        {
            return false;
        }

        foreach (string id in boosts)
        {
            inventory.TryUse(id);
        }

        SelectedLevel = level;
        attemptBoosts = boosts;
        return true;
    }

    // Gameplay takes the launch selection once. A scene reload cannot reuse it.
    public static IReadOnlyList<string> TakeSelectedBoosts()
    {
        var selected = attemptBoosts;
        attemptBoosts = Array.Empty<string>();
        return selected;
    }

    public static string BestStarsKey(int level) => $"level_{level}_best_stars";
}
