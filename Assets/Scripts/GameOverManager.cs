using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public GameObject GameOverPanel;

    public TextMeshProUGUI yourScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Game Over Audio")]
    [SerializeField] private AudioSource gameOverAudio;
    [SerializeField] private AudioClip gameOverClip;

    public void ShowGameOver()
    {
        // 🔊 Play Game Over sound
        if (gameOverAudio == null)
            gameOverAudio = GetComponent<AudioSource>();

        if (gameOverClip != null)
        {
            if (gameOverAudio != null)
            {
                gameOverAudio.PlayOneShot(gameOverClip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(gameOverClip, transform.position);
            }
        }

        GameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        MiningController mining = FindObjectOfType<MiningController>();

        int score = 0;

        if (mining != null)
        {
            score = mining.GetCobaltCount();
        }

        yourScoreText.text = "Your Score: " + score;
        if (score > ScoreManager.instance.highScore)
        {
            ScoreManager.instance.highScore = score;
            PlayerPrefs.SetInt("HighScore", score);
        }

        highScoreText.text = "High Score: " + ScoreManager.instance.highScore;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        ScoreManager.instance.ResetScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}