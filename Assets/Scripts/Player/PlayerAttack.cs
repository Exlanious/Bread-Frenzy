using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Transform))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Settings")]
    public KeyCode attackKey = KeyCode.Mouse0;
    public float attackDuration = 0.15f;
    public float attackCooldown = 0.25f;
    public float knockbackForce = 8f; 
    public float upwardBias = 0.4f;    

    [Header("References (assign in inspector)")]
    public Collider attackCollider;               
    public CollisionBroadcaster hitboxBroadcaster; 
    [Tooltip("Optional visual mesh to flash when attacking.")]
    public MeshRenderer attackRenderer;

    [Header("Hit Filter")]
    public LayerMask hittableLayers = ~0; 

    [Header("Stats")]
    public PlayerStats playerStats;
    [Header("Slash Prefab")]
    public GameObject slashPrefab;
    public float slashSpawnDistance = 1.5f;


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
        if (slashPrefab == null)
        {
            Debug.LogWarning("PlayerAttack: No slashPrefab assigned!");
            yield break;
        }

        canAttack = false;
        isAttacking = true;

        Vector3 spawnPos = transform.position + Vector3.up * 0.1f;
        Quaternion spawnRot = Quaternion.LookRotation(transform.forward, Vector3.up);

        GameObject slashInstance = Instantiate(slashPrefab, spawnPos, spawnRot);
        slashInstance.transform.SetParent(transform, true);

        float radiusMult = 1f;
        if (playerStats != null)
            radiusMult = playerStats.radius;

        slashInstance.transform.localScale *= radiusMult;

        CollisionBroadcaster broadcaster = slashInstance.GetComponent<CollisionBroadcaster>();
        if (broadcaster != null)
        {
            broadcaster.OnTriggerEnterEvent += OnHitboxTriggerEnter;
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;

        if (broadcaster != null)
        {
            broadcaster.OnTriggerEnterEvent -= OnHitboxTriggerEnter;
        }

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }


    private void OnHitboxTriggerEnter(Collider other)
    {
        if (!isAttacking) return;
        if (other == null || other.gameObject == gameObject) return;

        if (((1 << other.gameObject.layer) & hittableLayers) == 0) return;

        Vector3 dir = (other.transform.position - transform.position);
        dir.y = 0f;
        dir = dir.normalized;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            int dmg = 1;
            if (playerStats != null)
                dmg = Mathf.Max(1, Mathf.RoundToInt(playerStats.damage));

            enemyHealth.TakeDamage(dmg, dir); 
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
            radiusMult = playerStats.radius;

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
