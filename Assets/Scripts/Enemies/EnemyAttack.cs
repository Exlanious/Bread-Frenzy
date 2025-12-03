using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    protected PlayerHealth playerHealth;

    [Header("State")]
    public bool isKnockedBack = false;

    [Header("Attack Settings")]
    public int damage = 1;
    public float attackCooldown = 0.8f;
    public float attackRange = 1.6f;

    [Header("Attack Timing (no anims)")]
    [Tooltip("How long the enemy 'winds up' before the hit check.")]
    public float windupTime = 0.25f;
    [Tooltip("Small window where damage can happen once if you're still in range.")]
    public float hitWindow = 0.1f;

    protected float lastAttackTime = -999f;
    protected bool isAttacking = false;

    [Header("Optional Visual Telegraph")]
    [Tooltip("Renderer to tint during windup (optional).")]
    public Renderer telegraphRenderer;
    public Color telegraphColor = new Color(1f, 0.3f, 0.3f, 1f);

    private Color _originalColor;
    private bool _hasOriginalColor = false;

    public void SetKnockedBack(bool value)
    {
        isKnockedBack = value;
    }

    protected void Awake()
    {
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
            player = playerHealth.transform;

        if (telegraphRenderer != null && telegraphRenderer.material != null)
        {
            _originalColor = telegraphRenderer.material.color;
            _hasOriginalColor = true;
        }
    }

    protected void Update()
    {
        if (player == null || playerHealth == null) return;

        if (isKnockedBack) return;

        Vector3 delta = player.position - transform.position;
        delta.y = 0f;                       
        float distance = delta.magnitude;   

        if (!isAttacking &&
            distance <= attackRange &&
            Time.time - lastAttackTime >= attackCooldown)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        isAttacking = true;

        float t = 0f;

        if (telegraphRenderer != null && _hasOriginalColor)
        {
            telegraphRenderer.material.color = telegraphColor;
        }

        while (t < windupTime)
        {
            if (isKnockedBack)
            {
                CancelAttack();
                yield break;
            }

            t += Time.deltaTime;

            if (player != null)
            {
                Vector3 lookTarget = new Vector3(player.position.x, transform.position.y, player.position.z);
                transform.LookAt(lookTarget);
            }

            yield return null;
        }

        bool hasHit = false;
        float hitT = 0f;

        while (hitT < hitWindow)
        {
            if (isKnockedBack)
            {
                CancelAttack();
                yield break;
            }

            hitT += Time.deltaTime;

            if (!hasHit && playerHealth != null && player != null)
            {
                Vector3 delta = player.position - transform.position;
                delta.y = 0f;                       
                float distance = delta.magnitude;  

                if (distance <= attackRange)
                {
                    Debug.Log($"[EnemyAttack:{name}] Attacking player for {damage} damage.");
                    playerHealth.TakeDamage(damage);
                    hasHit = true;
                }
            }

            yield return null;
        }

        lastAttackTime = Time.time;

        if (telegraphRenderer != null && _hasOriginalColor)
        {
            telegraphRenderer.material.color = _originalColor;
        }

        isAttacking = false;
    }

    private void CancelAttack()
    {
        if (telegraphRenderer != null && _hasOriginalColor)
        {
            telegraphRenderer.material.color = _originalColor;
        }

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
