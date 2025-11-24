using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public float attackDuration = 0.15f;
    public float attackCooldown = 0.25f;
    public float knockbackForce = 8f;  // currently not used, kept for future if you want
    public float upwardBias = 0.4f;    // same as above

    [Header("References (assign in inspector)")]
    public Collider attackCollider;                 // hitbox (isTrigger)
    public CollisionBroadcaster hitboxBroadcaster;  // broadcaster component on hitbox
    [Tooltip("Optional visual mesh to flash when attacking.")]
    public MeshRenderer attackRenderer;

    [Header("Hit Filter")]
    public LayerMask hittableLayers = ~0; // everything by default

    [Header("Stats")]
    public PlayerStats playerStats;

    // Internal state
    private bool isAttacking;
    private bool canAttack = true;

    // Radius scaling cache
    private bool radiusInitialized = false;
    private float baseRadius = 1f;
    private Vector3 baseColliderScale = Vector3.one;

    void Awake()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        if (attackRenderer != null)
            attackRenderer.enabled = false;

        // Auto-find PlayerStats if not assigned
        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
    }

    void OnEnable()
    {
        if (hitboxBroadcaster != null)
            hitboxBroadcaster.OnTriggerEnterEvent += OnHitboxTriggerEnter;
    }

    void OnDisable()
    {
        if (hitboxBroadcaster != null)
            hitboxBroadcaster.OnTriggerEnterEvent -= OnHitboxTriggerEnter;
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && canAttack)
            StartCoroutine(PerformAttack());
    }

    IEnumerator PerformAttack()
    {
        canAttack = false;
        isAttacking = true;

        // Update hitbox size based on PlayerStats.radius
        ApplyRadiusFromStats();

        if (attackCollider != null)
            attackCollider.enabled = true;

        if (attackRenderer != null)
            attackRenderer.enabled = true;

        yield return new WaitForSeconds(attackDuration);

        if (attackCollider != null)
            attackCollider.enabled = false;

        if (attackRenderer != null)
            attackRenderer.enabled = false;

        isAttacking = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    private void OnHitboxTriggerEnter(Collider other)
    {
        if (!isAttacking) return;
        if (other == null || other.gameObject == gameObject) return;

        // Layer filter
        if (((1 << other.gameObject.layer) & hittableLayers) == 0) return;

        // Direction from player to enemy (horizontal)
        Vector3 dir = (other.transform.position - transform.position);
        dir.y = 0f;
        dir = dir.normalized;

        // Damage from PlayerStats
        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            int dmg = 1;
            if (playerStats != null)
                dmg = Mathf.Max(1, Mathf.RoundToInt(playerStats.damage));

            enemyHealth.TakeDamage(dmg, dir); // uses your knockback-aware overload
        }
    }

    // ------- Radius helpers -------

    void InitRadiusCache()
    {
        if (radiusInitialized || attackCollider == null)
            return;

        radiusInitialized = true;

        if (attackCollider is SphereCollider sphere)
        {
            baseRadius = sphere.radius;
        }
        else
        {
            baseColliderScale = attackCollider.transform.localScale;
        }
    }

    void ApplyRadiusFromStats()
    {
        if (attackCollider == null)
            return;

        InitRadiusCache();

        float radiusMult = 1f;
        if (playerStats != null)
            radiusMult = playerStats.radius; // 1 = default, 1.5 = 50% bigger, etc.

        if (attackCollider is SphereCollider sphere)
        {
            sphere.radius = baseRadius * radiusMult;
        }
        else
        {
            attackCollider.transform.localScale = baseColliderScale * radiusMult;
        }
    }
}
