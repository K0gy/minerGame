using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int currentScore = 0;
    public int highScore = 0;

    private OreCounterUI oreUI;

    private void Awake()
    {
        instance = this;
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    private void Start()
    {
        oreUI = FindObjectOfType<OreCounterUI>();
        UpdateUI();
    }

    public void AddCobalt(int amount)
    {
        currentScore += amount;
        UpdateUI();
    }

    public void ResetScore()
    {
        currentScore = 0;
        UpdateUI();
    }

    public void CheckHighScore()
    {
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    private void UpdateUI()
    {
        if (oreUI != null)
        {
            oreUI.SetCobaltCount(currentScore);
        }
    }
}