using System;

public class BoostService : IBoostService
{
    private readonly ISaveService save;
    private readonly EconomyConfig economy;

    public BoostService(ISaveService save, EconomyConfig economy)
    {
        this.save = save;
        this.economy = economy;
    }

    private string GetKey(string boostId)
    {
        if (boostId != "extra_time" &&
            boostId != "defense_bubble" &&
            boostId != "remove_item")
        {
            throw new ArgumentException("Unknown boost ID");
        }

        return $"boost_{boostId}_quantity";
    }

    public int GetQuantity(string boostId)
    {
        return save.GetInt(GetKey(boostId), 0);
    }

    public int GetPrice(string boostId)
    {
        return boostId switch
        {
            "extra_time" => economy.extraTimePrice,
            "defense_bubble" => economy.defenseBubblePrice,
            "remove_item" => economy.removeItemPrice,
            _ => throw new ArgumentException("Unknown boost ID")
        };
    }

    public bool CanBuy(string boostId)
    {
        int balance = save.GetInt("coins");

        return balance >= GetPrice(boostId);
    }

    public bool TryBuy(string boostId)
    {
        if (!CanBuy(boostId))
            return false;

        int price = GetPrice(boostId);
        int balance = save.GetInt("coins");
        int quantity = GetQuantity(boostId);

        save.SetInt("coins", balance - price);
        save.SetInt(GetKey(boostId), quantity + 1);

        return true;
    }

    public bool TryUse(string boostId)
    {
        int quantity = GetQuantity(boostId);

        if (quantity <= 0)
            return false;

        save.SetInt(GetKey(boostId), quantity - 1);

        return true;
    }
}