using System;

public class LifeService : ILifeService
{
    const string FullAtKey = "lives_full_at";
    const string SavedRecoveryIntervalKey = "lives_recovery_interval";
    const string PurchaseDayKey = "lives_purchase_day";
    const string PurchaseCountKey = "lives_purchase_count";
    const string CoinsKey = "coins";
    // Legacy saves encoded a fixed recovery interval and four possible purchase counts per day.
    const string LegacyPurchaseRecordKey = "lives_daily_purchases";
    const int LegacyRecoveryInterval = 600;
    const int LegacyPurchaseValuesPerDay = 4;
    static readonly DateTimeOffset SaveEpoch = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero);

    readonly ISaveService save;
    readonly EconomyConfig economy;
    readonly Func<DateTimeOffset> utcNow;

    public LifeService(ISaveService save, EconomyConfig economy, Func<DateTimeOffset> utcNow = null)
    {
        this.save = save;
        this.economy = economy;
        this.utcNow = utcNow ?? (() => DateTimeOffset.UtcNow);
    }

    public int MaxLives => Math.Max(1, economy.maxLives);
    public int DailyPurchaseLimit => Math.Max(0, economy.dailyLifePurchaseLimit);
    public int Price => economy.extraLifePrice;
    public int CurrentLives => MaxLives - ReadRecovery(SecondsSinceEpoch()).missing;
    public int PurchasesToday => PurchasesOn(UtcDay(SecondsSinceEpoch()));
    public int SecondsUntilNextLife => ReadRecovery(SecondsSinceEpoch()).next;

    public bool CanBuyLife => CurrentLives < MaxLives && PurchasesToday < DailyPurchaseLimit &&
        save.GetInt(CoinsKey) >= Price;

    public bool TryBuyLife()
    {
        int now = SecondsSinceEpoch();
        var recovery = ReadRecovery(now);
        int day = UtcDay(now);
        int purchases = PurchasesOn(day);
        int balance = save.GetInt(CoinsKey);
        if (recovery.missing == 0 || purchases >= DailyPurchaseLimit || balance < Price)
        {
            return false;
        }

        SaveRecovery(now, recovery.missing - 1, recovery.next);
        save.SetInt(PurchaseCountKey, purchases + 1);
        if (save.GetInt(PurchaseDayKey, -1) != day)
        {
            save.SetInt(PurchaseDayKey, day);
        }
        save.SetInt(CoinsKey, balance - Price);
        return true;
    }

    public bool TryLoseLife()
    {
        int now = SecondsSinceEpoch();
        var recovery = ReadRecovery(now);
        if (recovery.missing >= MaxLives)
        {
            return false;
        }

        SaveRecovery(now, recovery.missing + 1, recovery.next);
        return true;
    }

    // The time when the pool will be full also encodes the number of missing lives.
    // Decode using the interval in effect when that deadline was written.
    (int missing, int next) ReadRecovery(int now)
    {
        long remaining = (long)save.GetInt(FullAtKey) - now;
        if (remaining <= 0)
        {
            return (0, 0);
        }

        int interval = Math.Max(1, save.GetInt(SavedRecoveryIntervalKey, LegacyRecoveryInterval));
        int missing = (int)Math.Min(MaxLives, (remaining + interval - 1) / interval);
        int next = (int)Math.Min(remaining - (missing - 1L) * interval, int.MaxValue);
        return (missing, next);
    }

    void SaveRecovery(int now, int missing, int next)
    {
        if (missing == 0)
        {
            save.SetInt(FullAtKey, now);
            return;
        }

        int interval = Math.Max(1, economy.lifeRegenerationSeconds);
        int timeToNext = next > 0 ? Math.Min(next, interval) : interval;
        long fullAt = (long)now + timeToNext + (missing - 1L) * interval;
        if (save.GetInt(SavedRecoveryIntervalKey, LegacyRecoveryInterval) != interval)
        {
            save.SetInt(SavedRecoveryIntervalKey, interval);
        }
        save.SetInt(FullAtKey, (int)Math.Min(fullAt, int.MaxValue));
    }

    int SecondsSinceEpoch() => (int)(utcNow() - SaveEpoch).TotalSeconds;
    static int UtcDay(int seconds) => seconds / (24 * 60 * 60);

    int PurchasesOn(int day)
    {
        int savedDay = save.GetInt(PurchaseDayKey, -1);
        if (savedDay >= 0)
        {
            return savedDay == day ? save.GetInt(PurchaseCountKey) : 0;
        }

        int record = save.GetInt(LegacyPurchaseRecordKey);
        return record / LegacyPurchaseValuesPerDay == day ? record % LegacyPurchaseValuesPerDay : 0;
    }
}
