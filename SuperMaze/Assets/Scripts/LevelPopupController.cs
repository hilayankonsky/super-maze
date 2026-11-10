using UnityEngine;
using TMPro;

public class LevelPopupController : MonoBehaviour
{
    [SerializeField] GameObject levelPopup;
    [SerializeField] TMP_Text levelTitle;

    void Start()
    {
        Close();
    }

    public void Open(int levelNumber)
    {
        levelTitle.text = "Level " + levelNumber;
        levelPopup.SetActive(true);
    }

    public void Close()
    {
        levelPopup.SetActive(false);
    }
}