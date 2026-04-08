using UnityEngine;

public class FallingTileDamage : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private bool hasDamagedPlayer = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasDamagedPlayer) return;

        Debug.Log("Falling tile trigger hit: " + other.name);

        PlayerHealth playerHealth = other.GetComponentInParent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasDamagedPlayer = true;
            Debug.Log("Falling tile damaged player for " + damage);
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}