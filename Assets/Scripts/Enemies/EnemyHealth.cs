using UnityEngine;
using UnityEngine.AI;
using System;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("UI")]
    public ProgressBar healthBar;
    public bool hideWhenFull = true;

    [Header("Death Settings")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 0.5f;

    [Header("Hit Feedback")]
    public bool enableHitFlash = true;
    public Renderer meshRenderer;               // works with MeshRenderer or SkinnedMeshRenderer
    public Color hitColor = Color.red;
    public float flashDuration = 0.08f;

    [Header("Hit Stun / Knockback")]
    public bool enableHitStun = true;
    public float hitStunTime = 0.1f;

    [Header("Knockback")]
    public bool enableKnockback = true;
    public float knockbackDistance = 2f;
    public float knockbackDuration = 0.2f;   // a bit longer

    // NEW:
    public float verticalBump = 0.4f;        // little hop
    public AnimationCurve knockbackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);


    public event Action<EnemyHealth> OnEnemyDied;

    [Header("XP Settings")]
    public int xpValue = 1;   // how much XP this enemy gives

    public static System.Action<int> OnAnyEnemyDied; // xpValue passed to listeners


    // cached
    private Material[] materials;
    private Color[] originalColors;
    private bool isFlashing;

    private NavMeshAgent agent;
    private bool inKnockback;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();

        if (meshRenderer == null)
            meshRenderer = GetComponentInChildren<Renderer>();

        if (meshRenderer != null)
        {
            materials = meshRenderer.materials;
            originalColors = new Color[materials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i].HasProperty("_BaseColor"))
                    originalColors[i] = materials[i].GetColor("_BaseColor");
                else if (materials[i].HasProperty("_Color"))
                    originalColors[i] = materials[i].GetColor("_Color");
            }
        }

        agent = GetComponent<NavMeshAgent>();
    }

    // OLD signature kept for anything else that calls it
    public void TakeDamage(int amount)
    {
        TakeDamage(amount, Vector3.zero);
    }

    // NEW: damage with knockback direction
    public void TakeDamage(int amount, Vector3 hitDirection)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        currentHealth = Mathf.Max(currentHealth, 0);

        UpdateHealthBar();

        // visual + movement reaction
        StartCoroutine(HitReactionRoutine(hitDirection));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitReactionRoutine(Vector3 hitDirection)
    {
        // stop agent while reacting / being knocked back
        if (enableHitStun && agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (enableHitFlash && !isFlashing && materials != null && originalColors != null)
            StartCoroutine(FlashRoutine());

        if (enableKnockback && hitDirection != Vector3.zero)
            yield return StartCoroutine(KnockbackRoutine(hitDirection));
        else
            yield return new WaitForSeconds(hitStunTime);

        if (enableHitStun && agent != null && currentHealth > 0)
            agent.isStopped = false;
    }

    private IEnumerator FlashRoutine()
    {
        isFlashing = true;

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].HasProperty("_BaseColor"))
                materials[i].SetColor("_BaseColor", hitColor);
            else if (materials[i].HasProperty("_Color"))
                materials[i].SetColor("_Color", hitColor);
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < materials.Length; i++)
        {
            if (materials[i].HasProperty("_BaseColor"))
                materials[i].SetColor("_BaseColor", originalColors[i]);
            else if (materials[i].HasProperty("_Color"))
                materials[i].SetColor("_Color", originalColors[i]);
        }

        isFlashing = false;
    }

    private IEnumerator KnockbackRoutine(Vector3 hitDirection)
    {
        if (inKnockback) yield break;
        inKnockback = true;

        // flatten direction
        hitDirection.y = 0f;
        hitDirection.Normalize();

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + hitDirection * knockbackDistance;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            float t = elapsed / knockbackDuration;

            // ease using curve instead of raw t
            float eased = knockbackCurve.Evaluate(t);

            // base horizontal knockback
            Vector3 pos = Vector3.Lerp(startPos, targetPos, eased);

            // add a little arc (up then down)
            float height = Mathf.Sin(t * Mathf.PI) * verticalBump;
            pos.y = startPos.y + height;

            transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        inKnockback = false;
    }

    private void UpdateHealthBar()
    {
        if (healthBar == null) return;

        float progress = (float)currentHealth / maxHealth;
        healthBar.SetProgress(progress);

        if (hideWhenFull)
            healthBar.gameObject.SetActive(progress < 1f);
    }

    private void Die()
    {
        if (healthBar != null)
            healthBar.gameObject.SetActive(false);

        // Start death animation
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        float duration = 0.3f;          // how long the effect lasts
        float elapsed = 0f;

        Vector3 startScale = transform.localScale;

        // Cache materials so we can fade them
        Material[] mats = meshRenderer != null ? meshRenderer.materials : null;
        Color[] startColors = null;

        if (mats != null)
        {
            startColors = new Color[mats.Length];
            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_BaseColor"))
                    startColors[i] = mats[i].GetColor("_BaseColor");
                else if (mats[i].HasProperty("_Color"))
                    startColors[i] = mats[i].GetColor("_Color");
            }
        }

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            // Scale down
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            // Fade out
            if (mats != null)
            {
                for (int i = 0; i < mats.Length; i++)
                {
                    Color c = startColors[i];
                    c.a = Mathf.Lerp(1f, 0f, t);

                    if (mats[i].HasProperty("_BaseColor"))
                        mats[i].SetColor("_BaseColor", c);
                    else if (mats[i].HasProperty("_Color"))
                        mats[i].SetColor("_Color", c);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Make sure it ends fully invisible + tiny
        transform.localScale = Vector3.zero;

        if (destroyOnDeath)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }


    void OnDestroy()
    {
        OnEnemyDied?.Invoke(this);
        OnAnyEnemyDied?.Invoke(xpValue);
    }
}
