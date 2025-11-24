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
    public Collider attackCollider;                 // hitbox (isTrigger)
    public CollisionBroadcaster hitboxBroadcaster;  // broadcaster component on hitbox
    [Tooltip("Optional visual mesh to flash when attacking.")]
    public MeshRenderer attackRenderer;

    [Header("Hit Filter")]
    public LayerMask hittableLayers = ~0; // everything by default

    private bool isAttacking;
    private bool canAttack = true;

    void Awake()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        if (attackRenderer != null)
            attackRenderer.enabled = false;
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

        // Direction from player to enemy
        Vector3 dir = (other.transform.position - transform.position);
        dir.y = 0f; // flatten so it's horizontal
        dir = dir.normalized;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(1, dir);   // use new overload with direction
        }
    }
}
