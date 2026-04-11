using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Life")]
    [SerializeField] private int startingLives = 3;

    [Header("Invincibility Frames")]
    [SerializeField] private float invincibilityDuration = 0.75f;

    [Header("References")]
    [SerializeField] private HealthUI healthUI;
    [SerializeField] private GameOverManager gameOverManager;

    private int currentLives;
    private bool isInvincible;
    private Coroutine invincibilityRoutine;

    private void Start()
    {
        currentLives = startingLives;
        UpdateUI();
    }

    public void TakeDamage(int amount)
    {
        if (isInvincible) return;

        currentLives -= amount;

        // Empêcher < 0
        if (currentLives < 0)
            currentLives = 0;

        UpdateUI();

        Debug.Log("Player life: " + currentLives);

        // Game Over
        if (currentLives == 0)
        {
            if (gameOverManager != null)
            {
                gameOverManager.ShowGameOver();
            }
        }

        if (invincibilityRoutine != null)
        {
            StopCoroutine(invincibilityRoutine);
        }

        invincibilityRoutine = StartCoroutine(InvincibilityCoroutine());
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    private void UpdateUI()
    {
        if (healthUI != null)
        {
            healthUI.SetLives(currentLives);
        }
    }

    private IEnumerator InvincibilityCoroutine()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        invincibilityRoutine = null;
    }
}