using UnityEngine;
using System.Collections;

public class RangedEnemyAttack : MonoBehaviour, IEnemyAttack
{
    [Header("Target")]
    public Transform player;
    private PlayerHealth playerHealth;

    [Header("State")]
    public bool isKnockedBack = false;

    [Header("Attack Settings")]
    public int damage = 1;
    public float attackCooldown = 1.2f;
    public float attackRange = 10f;

    [Header("Attack Timing")]
    public float windupTime = 0.3f;
    public float projectileSpawnDelay = 0f; 

    [Header("Telegraph (optional)")]
    public Renderer telegraphRenderer;
    public Color telegraphColor = new Color(0.4f, 0.9f, 1f, 1f);

    private Color _originalColor;
    private bool _hasOriginalColor;

    [Header("Projectile")]
    public EnemyProjectile projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 30f;  
    public float verticalAimOffset = 0.5f;

    private float lastAttackTime = -999f;
    private bool isAttacking = false;

    public void SetKnockedBack(bool value)
    {
        isKnockedBack = value;
    }

    void Awake()
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

    void Update()
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

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (telegraphRenderer != null && _hasOriginalColor)
            telegraphRenderer.material.color = telegraphColor;

        float t = 0f;

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
                Vector3 lookTarget = new Vector3(
                    player.position.x,
                    transform.position.y,
                    player.position.z
                );
                transform.LookAt(lookTarget);
            }

            yield return null;
        }

        if (projectileSpawnDelay > 0f)
            yield return new WaitForSeconds(projectileSpawnDelay);

        if (!isKnockedBack && player != null && projectilePrefab != null && firePoint != null)
        {
            Vector3 targetPos = player.position + Vector3.up * verticalAimOffset;
            Vector3 dir = (targetPos - firePoint.position).normalized;

            EnemyProjectile proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(dir));
            proj.damage = damage;
            proj.speed = projectileSpeed;
            proj.Initialize(dir);
        }

        lastAttackTime = Time.time;

        if (telegraphRenderer != null && _hasOriginalColor)
            telegraphRenderer.material.color = _originalColor;

        isAttacking = false;
    }

    private void CancelAttack()
    {
        if (telegraphRenderer != null && _hasOriginalColor)
            telegraphRenderer.material.color = _originalColor;

        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
