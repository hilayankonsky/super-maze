using System;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LevelPopupController : MonoBehaviour
{
    [Serializable]
    class BonusOption
    {
        public string id;
        public Button button;
        public TMP_Text quantity;
        [NonSerialized] public bool selected;
    }

    [SerializeField] GameObject levelPopup;
    [SerializeField] TMP_Text levelTitle;
    [SerializeField] ScreenFlow screenFlow;
    [SerializeField] Button playButton;
    [SerializeField] BonusOption[] bonuses = Array.Empty<BonusOption>();
    [SerializeField] Color selectedBonusColor = new Color32(255, 210, 105, 255);
    [SerializeField] TMP_Text boostStatus;

    int openedLevel;
    bool starting;

    void Start()
    {
        Close();
    }

    public void Open(int levelNumber)
    {
        if (starting || !LevelProgression.IsUnlocked(levelNumber))
        {
            return;
        }

        openedLevel = levelNumber;
        levelTitle.text = "Level " + levelNumber;
        SetBoostStatus(string.Empty);
        foreach (var bonus in bonuses)
        {
            bonus.selected = false;
            RefreshBonus(bonus);
        }

        levelPopup.SetActive(true);
        UpdatePlayButton();
    }

    public void PlaySelectedLevel()
    {
        if (starting || !levelPopup.activeSelf || screenFlow == null)
        {
            return;
        }

        starting = true;
        if (!LevelProgression.TrySelect(openedLevel, bonuses.Where(bonus => bonus.selected).Select(bonus => bonus.id)))
        {
            starting = false;
            bool hasUnavailableSelection = false;
            foreach (var bonus in bonuses)
            {
                if (bonus.selected && Quantity(bonus) < 1)
                {
                    bonus.selected = false;
                    hasUnavailableSelection = true;
                }

                RefreshBonus(bonus);
            }

            if (hasUnavailableSelection)
            {
                SetBoostStatus("A selected boost is out of stock.");
            }

            UpdatePlayButton();
            return;
        }

        Close();
        screenFlow.Play();
    }

    public void ToggleBonus(int index)
    {
        if (starting || !levelPopup.activeSelf || index < 0 || index >= bonuses.Length)
        {
            return;
        }

        var bonus = bonuses[index];
        if (!bonus.selected && Quantity(bonus) < 1)
        {
            SetBoostStatus("You don't own this boost.");
        }
        else
        {
            bonus.selected = !bonus.selected;
            SetBoostStatus(string.Empty);
        }

        RefreshBonus(bonus);
    }

    void Update()
    {
        if (levelPopup.activeSelf)
        {
            UpdatePlayButton();
        }
    }

    void UpdatePlayButton()
    {
        if (playButton != null)
        {
            playButton.interactable = Services.Lives != null && Services.Lives.CurrentLives > 0;
        }
    }

    void RefreshBonus(BonusOption bonus)
    {
        bonus.quantity.text = Quantity(bonus).ToString();
        bonus.button.targetGraphic.color = bonus.selected ? selectedBonusColor : Color.white;
    }

    static int Quantity(BonusOption bonus) => Services.Boosts?.GetQuantity(bonus.id) ?? 0;

    void SetBoostStatus(string message)
    {
        if (boostStatus != null)
        {
            boostStatus.text = message;
        }
    }

    public void Close()
    {
        levelPopup.SetActive(false);
    }
}
