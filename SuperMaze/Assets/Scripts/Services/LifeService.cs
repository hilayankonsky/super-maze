using System;

public class LifeService : ILifeService
{
    const string FullAtKey = "lives_full_at";
    const string PurchaseRecordKey = "lives_daily_purchases";
    const string CoinsKey = "coins";
    const int RegenerationSeconds = 10 * 60;
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

    public int MaxLives => 3;
    public int DailyPurchaseLimit => 3;
    public int Price => economy.extraLifePrice;
    public int CurrentLives => MaxLives - MissingLives(RecoverySeconds(SecondsSinceEpoch()));
    public int PurchasesToday => PurchasesOn(UtcDay(SecondsSinceEpoch()));

    public int SecondsUntilNextLife
    {
        get
        {
            int remaining = RecoverySeconds(SecondsSinceEpoch());
            return remaining == 0 ? 0 : (remaining - 1) % RegenerationSeconds + 1;
        }
    }

    public bool CanBuyLife => CurrentLives < MaxLives && PurchasesToday < DailyPurchaseLimit &&
        save.GetInt(CoinsKey) >= Price;

    public bool TryBuyLife()
    {
        int now = SecondsSinceEpoch();
        int remaining = RecoverySeconds(now);
        int day = UtcDay(now);
        int purchases = PurchasesOn(day);
        int balance = save.GetInt(CoinsKey);
        if (remaining == 0 || purchases >= DailyPurchaseLimit || balance < Price)
        {
            return false;
        }

        save.SetInt(FullAtKey, now + Math.Max(0, remaining - RegenerationSeconds));
        save.SetInt(PurchaseRecordKey, day * (DailyPurchaseLimit + 1) + purchases + 1);
        save.SetInt(CoinsKey, balance - Price);
        return true;
    }

    public bool TryLoseLife()
    {
        int now = SecondsSinceEpoch();
        int remaining = RecoverySeconds(now);
        if (MissingLives(remaining) >= MaxLives)
        {
            return false;
        }

        save.SetInt(FullAtKey, now + remaining + RegenerationSeconds);
        return true;
    }

    // The time when the pool will be full also encodes the number of missing lives.
    // Each missing life adds one interval, so unattended regeneration needs no writes.
    int RecoverySeconds(int now) => Math.Clamp(save.GetInt(FullAtKey) - now, 0, MaxLives * RegenerationSeconds);

    static int MissingLives(int remaining) => (remaining + RegenerationSeconds - 1) / RegenerationSeconds;
    int SecondsSinceEpoch() => (int)(utcNow() - SaveEpoch).TotalSeconds;
    static int UtcDay(int seconds) => seconds / (24 * 60 * 60);

    // Store the UTC day and its count together to keep them in sync.
    int PurchasesOn(int day)
    {
        int record = save.GetInt(PurchaseRecordKey);
        int valuesPerDay = DailyPurchaseLimit + 1;
        return record / valuesPerDay == day ? record % valuesPerDay : 0;
    }
}
