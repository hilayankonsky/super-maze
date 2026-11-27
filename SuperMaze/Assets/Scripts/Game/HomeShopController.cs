using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeShopController : MonoBehaviour
{
    [Serializable]
    class Offer
    {
        public string id;
        public Button button;
        public TMP_Text price;
        public TMP_Text status;
        public TMP_Text effect;
        public Image[] progress;
    }

    [SerializeField] GameObject[] mapObjects;
    [SerializeField] GameObject shop;
    [SerializeField] GameObject boosts;
    [SerializeField] GameObject livesAndUpgrades;
    [SerializeField] Image boostsTab;
    [SerializeField] Image upgradesTab;
    [SerializeField] Sprite selectedTab;
    [SerializeField] Sprite inactiveTab;
    [SerializeField] Button lifePurchase;
    [SerializeField] TMP_Text lifePrice;
    [SerializeField] TMP_Text lifePurchaseStatus;
    [SerializeField] Offer[] boostOffers;
    [SerializeField] Offer[] upgradeOffers;

    ISaveService subscribedSave;
    float nextLivesRefresh;

    void OnEnable()
    {
        subscribedSave = Services.Save;
        if (subscribedSave != null)
        {
            subscribedSave.OnValueChanged += SavedValueChanged;
        }

        RefreshLives();
    }

    void OnDisable()
    {
        if (subscribedSave != null)
        {
            subscribedSave.OnValueChanged -= SavedValueChanged;
            subscribedSave = null;
        }
    }

    void Update()
    {
        if (Time.unscaledTime >= nextLivesRefresh)
        {
            nextLivesRefresh = Time.unscaledTime + 1f;
            RefreshLives();
        }
    }

    public void OpenShop()
    {
        if (shop.activeSelf)
        {
            return;
        }

        foreach (var item in mapObjects)
        {
            item.SetActive(false);
        }
        shop.SetActive(true);
        SelectBoosts();
        Refresh();
    }

    public void OpenHome()
    {
        if (!shop.activeSelf)
        {
            return;
        }

        shop.SetActive(false);
        foreach (var item in mapObjects)
        {
            item.SetActive(true);
        }
    }

    public void SelectBoosts() => SelectTab(true);

    public void SelectLivesAndUpgrades() => SelectTab(false);

    void SelectTab(bool showBoosts)
    {
        boosts.SetActive(showBoosts);
        livesAndUpgrades.SetActive(!showBoosts);
        boostsTab.sprite = showBoosts ? selectedTab : inactiveTab;
        upgradesTab.sprite = showBoosts ? inactiveTab : selectedTab;
    }

    public void BuyBoost(string id) => Services.Boosts.TryBuy(id);

    public void BuyUpgrade(string id) => Services.Upgrades.TryUpgrade(id);

    public void BuyLife()
    {
        if (shop.activeSelf && livesAndUpgrades.activeSelf)
        {
            Services.Lives.TryBuyLife();
            RefreshLives();
        }
    }

    void SavedValueChanged(string key)
    {
        if (shop.activeSelf && (key == "coins" || key.StartsWith("boost_", StringComparison.Ordinal) ||
            key.StartsWith("upgrade_", StringComparison.Ordinal)))
        {
            Refresh();
        }
    }

    void Refresh()
    {
        var economy = Services.Config.Economy;
        RefreshLives();
        foreach (var offer in boostOffers)
        {
            offer.price.text = Services.Boosts.GetPrice(offer.id).ToString();
            offer.status.text = $"Owned: {Services.Boosts.GetQuantity(offer.id)}";
            offer.button.interactable = Services.Boosts.CanBuy(offer.id);
        }

        foreach (var offer in upgradeOffers)
        {
            int level = Services.Upgrades.GetLevel(offer.id);
            bool atCap = level >= economy.maxUpgradeLevel;
            offer.price.text = atCap ? "MAX" : Services.Upgrades.GetPrice(offer.id).ToString();
            offer.status.text = $"Level {level} / {economy.maxUpgradeLevel}";
            offer.effect.text = OfferEffect(offer.id, economy, atCap);
            offer.button.interactable = Services.Upgrades.CanUpgrade(offer.id);
            for (int i = 0; i < offer.progress.Length; i++)
            {
                offer.progress[i].gameObject.SetActive(i < economy.maxUpgradeLevel);
                offer.progress[i].color = i < level ? new Color32(255, 200, 69, 255) : new Color32(143, 195, 216, 255);
            }
        }
    }

    void RefreshLives()
    {
        var lives = Services.Lives;
        if (lives == null)
        {
            return;
        }

        lifePrice.text = lives.Price.ToString();
        lifePurchaseStatus.text = $"Daily purchases: {lives.PurchasesToday}/{lives.DailyPurchaseLimit}";
        lifePurchase.interactable = lives.CanBuyLife;
    }

    static string OfferEffect(string id, EconomyConfig economy, bool atCap)
    {
        if (atCap)
        {
            return "Max level reached";
        }

        return id == "shoes"
            ? $"Next upgrade: +{economy.shoesSpeedIncrease:P0} speed"
            : $"Next upgrade: +{economy.flashlightRangeIncrease:P0} visibility";
    }
}
