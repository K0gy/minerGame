using System.Collections;
using UnityEngine;
using TarodevController;


public class GasExplosionController : MonoBehaviour
{
    [Header("Explosion Audio")]
    [SerializeField] private AudioSource explosionAudio;
    [SerializeField] private AudioClip explosionClip;
    [Header("References")]
    [SerializeField] private ProceduralGeneration proceduralGeneration;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Gas Trigger")]
    [SerializeField] private float gasDelay = 0.5f;

    [Header("Explosion")]
    [SerializeField] private int gasExplosionRadius = 2;
    [SerializeField] private float blastHorizontal = 20f;
    [SerializeField] private float blastVertical = 20f;
    [SerializeField] private float playerBlastRadius = 2.5f;
    [SerializeField] private bool destroyTerrainOnExplosion = true;

    [Header("Damage")]
    [SerializeField] private int explosionDamage = 3;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private Coroutine gasRoutine;
    private bool explosionPending;
    private Vector3Int pendingGasCell;

    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (proceduralGeneration == null)
            proceduralGeneration = FindFirstObjectByType<ProceduralGeneration>();
    }

    void Reset()
    {
        playerController = GetComponent<PlayerController>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (proceduralGeneration == null || playerController == null) return;

        Vector3Int playerCell = proceduralGeneration.WorldToCell(transform.position);
        bool inGas = proceduralGeneration.HasGas(playerCell.x, playerCell.y);

        if (inGas && !explosionPending)
        {
            pendingGasCell = playerCell;
            gasRoutine = StartCoroutine(GasCountdown());
        }
    }

    IEnumerator GasCountdown()
    {
        explosionPending = true;

        if (debugLogs) Debug.Log("Entered gas zone. Countdown started.");

        yield return new WaitForSeconds(gasDelay);

        TriggerExplosion(pendingGasCell.x, pendingGasCell.y);

        explosionPending = false;
        gasRoutine = null;
    }

    void TriggerExplosion(int gasX, int gasY)
    {
        if (proceduralGeneration == null) return;
        if (!proceduralGeneration.HasGas(gasX, gasY)) return;

        if (debugLogs) Debug.Log("BOOM!");
        if (explosionAudio != null && explosionClip != null)
        {
        explosionAudio.PlayOneShot(explosionClip);
        }

        Vector3 explosionWorldPos = new Vector3(gasX + 0.5f, gasY + 0.5f, 0f);

        proceduralGeneration.ExplodeGasAtCell(gasX, gasY, gasExplosionRadius, destroyTerrainOnExplosion);

        float distToPlayer = Vector2.Distance(transform.position, explosionWorldPos);
        if (distToPlayer > playerBlastRadius) return;

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(explosionDamage);
        }

        Vector2 dir = ((Vector2)transform.position - (Vector2)explosionWorldPos).normalized;

        if (dir.sqrMagnitude < 0.01f)
        {
            dir = new Vector2(Random.Range(-1f, 1f), 1f).normalized;
        }

        Vector2 launchVelocity = new Vector2(
            dir.x * blastHorizontal,
            Mathf.Abs(dir.y) * blastVertical + blastVertical * 0.35f
        );

        if (playerController != null)
        {
            playerController.AddExplosionVelocity(launchVelocity);
        }
    }
}