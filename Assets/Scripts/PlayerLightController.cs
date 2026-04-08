using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerLightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D playerLight;
    [SerializeField] private ProceduralGeneration proceduralGeneration;

    [Header("Battery")]
    [SerializeField] private float maxBattery = 10f;
    [SerializeField] private float currentBattery = 10f;
    [SerializeField] private float drainPerSecond = 1f;
    [SerializeField] private float rechargePerSecond = 2f;
    [SerializeField] private KeyCode rechargeKey = KeyCode.R;

    [Header("Recharge Rules")]
    [SerializeField] private float rechargeHoldTime = 1.0f;
    [SerializeField] private float movementThreshold = 0.05f;

    [Header("Light Intensity")]
    [SerializeField] private float minIntensity = 0f;
    [SerializeField] private float maxIntensity = 1.5f;

    [Header("Light Colors")]
    [SerializeField] private Color normalColor = new Color(1f, 0.92f, 0.65f, 1f);
    [SerializeField] private Color gasColor = new Color(0.55f, 0.75f, 1f, 1f);
    [SerializeField] private float colorLerpSpeed = 8f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private float rechargeHeldTimer = 0f;
    private Vector3 lastPosition;

    private void Awake()
    {
        if (playerLight == null)
            playerLight = GetComponentInChildren<Light2D>();

        if (proceduralGeneration == null)
            proceduralGeneration = FindFirstObjectByType<ProceduralGeneration>();

        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
        lastPosition = transform.position;
    }

    private void Update()
    {
        if (playerLight == null) return;

        HandleBattery();
        UpdateLightIntensity();
        UpdateLightColor();

        lastPosition = transform.position;
    }

    private void HandleBattery()
    {
        bool isHoldingRecharge = Input.GetKey(rechargeKey);
        bool isStandingStill = IsStandingStill();

        bool canCharge = isHoldingRecharge && isStandingStill;

        if (canCharge)
        {
            rechargeHeldTimer += Time.deltaTime;

            if (rechargeHeldTimer >= rechargeHoldTime)
            {
                currentBattery += rechargePerSecond * Time.deltaTime;
            }
        }
        else
        {
            rechargeHeldTimer = 0f;
            currentBattery -= drainPerSecond * Time.deltaTime;
        }

        currentBattery = Mathf.Clamp(currentBattery, 0f, maxBattery);
    }

    private bool IsStandingStill()
    {
        float movedDistance = Vector3.Distance(transform.position, lastPosition);
        return movedDistance <= movementThreshold;
    }

    private void UpdateLightIntensity()
    {
        float batteryPercent = maxBattery > 0f ? currentBattery / maxBattery : 0f;
        playerLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, batteryPercent);

        playerLight.enabled = currentBattery > 0.01f;
    }

    private void UpdateLightColor()
    {
        Color targetColor = normalColor;

        if (proceduralGeneration != null)
        {
            Vector3Int playerCell = proceduralGeneration.WorldToCell(transform.position);

            if (proceduralGeneration.HasGas(playerCell.x, playerCell.y))
            {
                targetColor = gasColor;
            }
        }

        playerLight.color = Color.Lerp(playerLight.color, targetColor, colorLerpSpeed * Time.deltaTime);
    }

    public float GetCurrentBattery()
    {
        return currentBattery;
    }

    public float GetMaxBattery()
    {
        return maxBattery;
    }

    public float GetRechargeProgress01()
    {
        if (rechargeHoldTime <= 0f) return 1f;
        return Mathf.Clamp01(rechargeHeldTimer / rechargeHoldTime);
    }

    public void RefillBatteryFull()
    {
        currentBattery = maxBattery;
        rechargeHeldTimer = 0f;

        if (debugLogs)
        {
            Debug.Log("Battery refilled.");
        }
    }
}