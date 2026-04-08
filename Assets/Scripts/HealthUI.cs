using TMPro;
using UnityEngine;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text livesText;

    public void SetLives(int currentLives)
    {
        if (livesText == null) return;
        livesText.text = "Lives: " + currentLives;
    }
}