public interface IBoostService
{
    int GetQuantity(string boostId);
    int GetPrice(string boostId);
    bool CanBuy(string boostId);
    bool TryBuy(string boostId);
    bool TryUse(string boostId);
}