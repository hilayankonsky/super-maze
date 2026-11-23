
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class LevelGameManager : MonoBehaviour
{
    [Header("Timer")]
    [SerializeField] private float levelDuration = 120f;
    [SerializeField] private TMP_Text timerText;

    [Header("End Game UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private TMP_Text completionTimeText;

    [Header("Win Rewards")]
    [SerializeField] private GameObject star1;
    [SerializeField] private GameObject star2;
    [SerializeField] private GameObject star3;
    [SerializeField] private TMP_Text rewardText;

    private float timeRemaining;
    private bool gameEnded;

    private void Start()
    {
        timeRemaining = levelDuration;
        gameEnded = false;

        winPanel.SetActive(false);
        losePanel.SetActive(false);

        UpdateTimerText();
    }

    private void Update()
    {
        if (gameEnded)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            LoseLevel();
        }

        UpdateTimerText();

        // Temporary testing shortcuts (New Input System)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.wasPressedThisFrame)
                WinLevel();

            if (Keyboard.current.lKey.wasPressedThisFrame)
                LoseLevel();
        }
    }

    private void UpdateTimerText()
    {
        int seconds = Mathf.CeilToInt(timeRemaining);
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        timerText.text = $"{minutes:00}:{remainingSeconds:00}";
    }

public void WinLevel()
{
    if (gameEnded)
        return;

    gameEnded = true;

    float completionTime = levelDuration - timeRemaining;

    int seconds = Mathf.FloorToInt(completionTime);
    int minutes = seconds / 60;
    int remainingSeconds = seconds % 60;

    completionTimeText.text =
        $"COMPLETION TIME: {minutes:00}:{remainingSeconds:00}";

    // Calculate stars based on completion time
    int stars;

    if (completionTime <= 40f)
        stars = 3;
    else if (completionTime <= 80f)
        stars = 2;
    else
        stars = 1;

    // Show the correct number of stars
    star1.SetActive(stars >= 1);
    star2.SetActive(stars >= 2);
    star3.SetActive(stars >= 3);

    // Calculate coin reward
    int coins = stars * 40;
    rewardText.text = $"+{coins} COINS";

    winPanel.SetActive(true);
}

    public void LoseLevel()
    {
        if (gameEnded)
            return;

        gameEnded = true;
        losePanel.SetActive(true);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene("Level");
    }

    public void GoHome()
    {
        SceneManager.LoadScene("Home");
    }
}
