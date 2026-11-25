public interface ILifeService
{
    int CurrentLives { get; }
    int MaxLives { get; }
    int PurchasesToday { get; }
    int DailyPurchaseLimit { get; }
    int SecondsUntilNextLife { get; }
    int Price { get; }
    bool CanBuyLife { get; }

    bool TryBuyLife();
    bool TryLoseLife();
}
