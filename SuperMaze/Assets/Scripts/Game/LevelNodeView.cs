using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class LevelNodeView : MonoBehaviour
{
    [SerializeField] int levelId = 1;
    [SerializeField] TMP_Text numberLabel;
    [SerializeField] Image[] stars = new Image[3];
    [SerializeField] Sprite starFull;
    [SerializeField] Sprite starEmpty;
    [SerializeField] Image levelArt;
    [SerializeField] Material lockedMaterial;

    ISaveService subscribedSave;

    public int LevelId => levelId;
    public int ShownStars { get; private set; }

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
        if (key == LevelProgression.BestStarsKey(levelId) ||
            (levelId > 1 && key == LevelProgression.BestStarsKey(levelId - 1)))
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

        bool unlocked = LevelProgression.IsUnlocked(levelId);
        GetComponent<Button>().interactable = unlocked;
        Material artMaterial = unlocked ? null : lockedMaterial;
        if (levelArt != null)
        {
            levelArt.material = artMaterial;
        }

        ShownStars = LevelProgression.BestStars(levelId);

        for (int i = 0; i < stars.Length; i++)
        {
            if (stars[i] != null)
            {
                stars[i].sprite = i < ShownStars ? starFull : starEmpty;
                stars[i].material = artMaterial;
            }
        }
    }
}
