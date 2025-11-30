using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Target")]
    public Transform player; // will be auto-filled

    [Header("Attack Settings")]
    public int damage = 1;
    public float attackCooldown = 0.5f;

    // Make this a bit larger than EnemyMoveAI.stopDistance (1.5f)
    public float attackRange = 2.0f;

    private float lastAttackTime = -999f;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        // Try to find the player health in the scene
        playerHealth = FindObjectOfType<PlayerHealth>();

        if (playerHealth != null)
        {
            player = playerHealth.transform;
            Debug.Log($"[EnemyAttack:{name}] Found PlayerHealth on {playerHealth.gameObject.name}");
        }
        else
        {
            Debug.LogError($"[EnemyAttack:{name}] No PlayerHealth found in scene. Enemy cannot attack.");
        }
    }

    private void Update()
    {
        if (player == null || playerHealth == null)
            return;

        float distance = Vector3.Distance(transform.position, player.position);

        // Debug distance so we see if ducks ever count as "close"
        // (Comment this out later if too spammy)
        // Debug.Log($"[EnemyAttack:{name}] Distance to player: {distance}");

        if (distance <= attackRange)
        {
            // We are in attack range – this should definitely show up
            Debug.Log($"[EnemyAttack:{name}] Player in range ({distance:F2}). Trying to attack.");
            TryAttackPlayer();
        }
    }

    private void TryAttackPlayer()
    {
        if (playerHealth == null) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Debug.Log($"[EnemyAttack:{name}] Attacking player for {damage} damage.");
            playerHealth.TakeDamage(damage);  // This calls your PlayerHealth logic :contentReference[oaicite:1]{index=1}
            lastAttackTime = Time.time;
        }
    }

    // Optional: visualize the attack radius in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
