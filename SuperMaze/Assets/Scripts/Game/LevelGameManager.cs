
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;

public class LevelGameManager : MonoBehaviour
{
    [Header("Timer")]
    private float levelDuration;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text levelTitleText;

    [Header("End Game UI")]
    [SerializeField] private GameObject endGameOverlay;
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
    private EconomyConfig economy;
    private int levelId;

    private void Start()
    {
        economy = Services.Config.Economy;
        levelDuration = economy.levelDuration;

        levelId = LevelProgression.SelectedLevel;

        if (levelTitleText != null)
            levelTitleText.text = $"LEVEL {levelId}";

        timeRemaining = levelDuration;
        gameEnded = false;

        winPanel.SetActive(false);
        losePanel.SetActive(false);
        endGameOverlay.SetActive(false);

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

        // Calculate stars and rewards using EconomyConfig
        int stars;
        int coins;

        if (completionTime <= economy.threeStarTime)
        {
            stars = 3;
            coins = economy.threeStarReward;
        }
        else if (completionTime <= economy.twoStarTime)
        {
            stars = 2;
            coins = economy.twoStarReward;
        }
        else
        {
            stars = 1;
            coins = economy.oneStarReward;
        }

        // Show the correct number of stars
        star1.SetActive(stars >= 1);
        star2.SetActive(stars >= 2);
        star3.SetActive(stars >= 3);

        // Load the best previous result
        string levelKey = LevelProgression.BestStarsKey(levelId);
        int bestStars = LevelProgression.BestStars(levelId);

        // Calculate the previous reward
        int previousReward = 0;

        switch (bestStars)
        {
            case 1:
                previousReward = economy.oneStarReward;
                break;

            case 2:
                previousReward = economy.twoStarReward;
                break;

            case 3:
                previousReward = economy.threeStarReward;
                break;
        }

        // Reward only improvements
        int earnedCoins = 0;

        if (stars > bestStars)
        {
            earnedCoins = Mathf.Max(0, coins - previousReward);

            int currentCoins = Services.Save.GetInt("coins");

            Services.Save.SetInt("coins", currentCoins + earnedCoins);
            Services.Save.SetInt(levelKey, stars);
        }

        // Display the actual earned reward
        rewardText.text = $"+{earnedCoins} COINS";

        endGameOverlay.SetActive(true);
        winPanel.SetActive(true);
    }

    public void LoseLevel()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        endGameOverlay.SetActive(true);
        losePanel.SetActive(true);
    }

    public void RetryLevel()
    {
        SceneManager.LoadScene("Level");
    }

    public void ContinueToNextLevel()
    {
        int nextLevel = levelId + 1;

        LevelProgression.RequestLevelPopup(nextLevel);

        SceneManager.LoadScene("Home");
    }

    public void GoHome()
    {
        SceneManager.LoadScene("Home");
    }
}
