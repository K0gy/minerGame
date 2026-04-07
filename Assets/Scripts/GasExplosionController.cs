using System.Collections;
using UnityEngine;
using TarodevController;

public class GasExplosionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ProceduralGeneration proceduralGeneration;
    [SerializeField] private PlayerController playerController;

    [Header("Gas Trigger")]
    [SerializeField] private float gasDelay = 0.5f;

    [Header("Explosion")]
    [SerializeField] private int gasExplosionRadius = 2;
    [SerializeField] private float blastHorizontal = 20f;
    [SerializeField] private float blastVertical = 20f;
    [SerializeField] private bool destroyTerrainOnExplosion = true;

    [Header("Player Blast Range")]
    [SerializeField] private float playerBlastRadius = 2.5f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private Coroutine gasRoutine;
    private bool explosionPending;
    private Vector3Int pendingGasCell;

    void Reset()
    {
        playerController = GetComponent<PlayerController>();
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
        if (!proceduralGeneration.HasGas(gasX, gasY)) return;

        if (debugLogs) Debug.Log("BOOM!");

        Vector3 explosionWorldPos = new Vector3(gasX + 0.5f, gasY + 0.5f, 0f);

        proceduralGeneration.ExplodeGasAtCell(gasX, gasY, gasExplosionRadius, destroyTerrainOnExplosion);

        float distToPlayer = Vector2.Distance(transform.position, explosionWorldPos);

        if (distToPlayer > playerBlastRadius)
        {
            if (debugLogs) Debug.Log("Explosion happened, but player was out of blast range.");
            return;
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

        playerController.AddExplosionVelocity(launchVelocity);
    }
}