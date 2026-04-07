using TMPro;
using UnityEngine;

public class OreCounterUI : MonoBehaviour
{
    [SerializeField] private TMP_Text cobaltText;

    private void Start()
    {
        SetCobaltCount(0);
    }

    public void SetCobaltCount(int amount)
    {
        if (cobaltText == null) return;
        cobaltText.text = "Cobalt: " + amount;
    }
}