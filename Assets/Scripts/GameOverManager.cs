using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject GameOverPanel;

    public TextMeshProUGUI yourScoreText;
    public TextMeshProUGUI highScoreText;

    public void ShowGameOver()
    {
        GameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        yourScoreText.text = "Your Score: " + ScoreManager.instance.currentScore;
        highScoreText.text = "High Score: " + ScoreManager.instance.highScore;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        ScoreManager.instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}