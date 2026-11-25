using TMPro;
using UnityEngine;
using UnityEngine.UI;

// LevelNodeView.cs (Assets/Scripts/Game): presentation only.
// Shows a Home map level node's number and its saved best star rating.
// The rating is read through Services.Save from the existing "level_{id}_best_stars" value
// that LevelGameManager writes. With no saved value it shows three empty stars.
// It does not change ratings, rewards, unlocking or the button's OnClick.
public class LevelNodeView : MonoBehaviour
{
    [SerializeField] int levelId = 1;
    [SerializeField] TMP_Text numberLabel;
    [SerializeField] Image[] stars = new Image[3];
    [SerializeField] Sprite starFull;
    [SerializeField] Sprite starEmpty;

    ISaveService subscribedSave;

    public int LevelId => levelId;
    public int ShownStars { get; private set; }

    string BestStarsKey => $"level_{levelId}_best_stars";

    void OnEnable()
    {
        subscribedSave = Services.Save;
        if (subscribedSave != null)
        {
            subscribedSave.OnValueChanged += HandleValueChanged;
        }

        Refresh();
    }

    void OnDisable()
    {
        if (subscribedSave != null)
        {
            subscribedSave.OnValueChanged -= HandleValueChanged;
            subscribedSave = null;
        }
    }

    void HandleValueChanged(string key)
    {
        if (key == BestStarsKey)
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (numberLabel != null)
        {
            numberLabel.text = levelId.ToString();
        }

        int best = Services.Save != null ? Mathf.Clamp(Services.Save.GetInt(BestStarsKey, 0), 0, 3) : 0;
        ShownStars = best;

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].sprite = i < best ? starFull : starEmpty;
            }
        }
    }
}
