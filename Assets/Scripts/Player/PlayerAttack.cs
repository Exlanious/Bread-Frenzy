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
    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 15f;
    public float projectileLifetime = 3f;
    public float projectileSpreadAngle = 10f;

    [Header("VFX")]
    public ParticleSystem hitEffect;

    [Header("Audio")]
    public AudioSource audioSource;
    [Header("Weapon Visuals")]
    public GameObject heldBaguette;

    public AudioClip swordSwingSound;
    public AudioClip projectileShootSound;
    public AudioClip hitSound;


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

        if (heldBaguette != null)
            heldBaguette.SetActive(false);

        Vector3 spawnPos = transform.position + Vector3.up * 0.1f;
        Quaternion spawnRot = Quaternion.LookRotation(transform.forward, Vector3.up);

        GameObject slashInstance = Instantiate(slashPrefab, spawnPos, spawnRot);
        slashInstance.transform.SetParent(transform, true);

        if (audioSource != null && swordSwingSound != null)
        {
            audioSource.PlayOneShot(swordSwingSound);
        }

        float radiusMult = 1f;
        if (playerStats != null)
            radiusMult = playerStats.radius;

        slashInstance.transform.localScale *= radiusMult;

        if (playerStats != null && playerStats.hasProjectile && projectilePrefab != null)
        {
            int count = Mathf.Max(1, playerStats.projectileCount);

            for (int i = 0; i < count; i++)
            {
                float spread = projectileSpreadAngle;
                float t = (count == 1) ? 0f : (i / (float)(count - 1) - 0.5f);
                float angleOffset = t * spread;

                Quaternion projRot = Quaternion.AngleAxis(angleOffset, Vector3.up) * spawnRot;

                GameObject projObj = Instantiate(projectilePrefab, spawnPos, projRot);

                Projectile proj = projObj.GetComponent<Projectile>();
                if (proj != null)
                {
                    int dmg = 1;
                    if (playerStats != null)
                        dmg = Mathf.Max(1, Mathf.RoundToInt(playerStats.damage));

                    proj.Initialize(
                        owner: transform,
                        damage: dmg,
                        speed: projectileSpeed,
                        lifetime: projectileLifetime,
                        hittableLayers: hittableLayers
                    );
                }

                if (audioSource != null && projectileShootSound != null)
                {
                    audioSource.PlayOneShot(projectileShootSound);
                }
            }
        }

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

        if (heldBaguette != null)
            heldBaguette.SetActive(true);

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

            float kb = playerStats.knockback;
            enemyHealth.TakeDamage(dmg, dir * kb);

            if (hitEffect != null)
            {
                hitEffect.transform.position = other.ClosestPoint(transform.position);
                hitEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                hitEffect.Play();
            }

            // Play hit sound
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }

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
