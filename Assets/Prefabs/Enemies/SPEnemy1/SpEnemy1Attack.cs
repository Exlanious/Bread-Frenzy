using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(EnemyMoveAI))]
public class SpEnemy1Attack : EnemyAttack
{
    public bool debugMode = false;

    [Header("Core")]
    public Animator animator;
    private Rigidbody rb;
    private EnemyMoveAI ai;

    [Header("Attack Pattern")]
    public float initialDelay = 5f;
    public float attackInterval = 5f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackBounceSound; // for Attack1 & normal bounces
    public AudioClip finalSmashSound;   // for final smash


    // -------------------------
    // Attack 1 (1 bounce)
    // -------------------------
    [Header("Attack 1 Settings")]
    public CollisionBroadcaster attack1Hitbox;
    public ParticleSystem attack1Particles;
    public float atk1JumpHeight = 10f;
    public float atk1JumpForce = 8f;
    public float atk1FallForce = 10f;
    public int atk1Damage = 1;
    public float atk1Knockback = 5f;

    // -------------------------
    // Attack 2 (3 bounces + smash)
    // -------------------------
    [Header("Attack 2 Bounce Settings")]
    public CollisionBroadcaster attack2BounceHitbox;
    public float atk2BounceHeight = 10f;
    public float atk2JumpForce = 10f;
    public float atk2FallForce = 12f;
    public int atk2BounceDamage = 1;

    [Header("Attack 2 Final Smash")]
    public CollisionBroadcaster attack2SmashHitbox;
    public ParticleSystem smashParticles;
    public float atk2SmashHeight = 18f;
    public float atk2SmashFallForce = 16f;
    public int atk2SmashDamage = 5;
    public float atk2SmashKnockback = 12f;

    private int bounceCounter = 0;
    private const int maxBounces = 3;
    private bool isFinalSmash = false;

    //private
    private bool trackPlayer = false;

    private void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        ai = GetComponent<EnemyMoveAI>();

        if (attack1Hitbox != null)
            attack1Hitbox.OnTriggerEnterEvent += Attack1OnHit;

        if (attack2BounceHitbox != null)
            attack2BounceHitbox.OnTriggerEnterEvent += Attack2BounceOnHit;

        if (attack2SmashHitbox != null)
            attack2SmashHitbox.OnTriggerEnterEvent += Attack2SmashOnHit;
    }

    void Start()
    {
        StartCoroutine(RandomAttackRoutine());
    }

    void Update()
    {
        if (debugMode)
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartAttack1();
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                StartAttack2();
            }
        }
        if (trackPlayer)
            MoveAbovePlayer(transform.position.y);
    }


    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator RandomAttackRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            yield return new WaitForSeconds(attackInterval);

            // Randomly choose 0 or 1
            int choice = Random.Range(0, 2);

            if (choice == 0)
                StartAttack1();
            else
                StartAttack2();
        }
    }


    // ================================
    // ATTACK 1 (Animation Based)
    // ================================
    public void StartAttack1()
    {
        animator.SetTrigger("Attack1");
    }

    // Animation Event
    public void A1_StartJump()
    {
        JumpStart(atk1JumpForce);
        trackPlayer = false;
    }

    // Animation Event
    public void A1_HoldHeight()
    {
        JumpHold(atk1JumpHeight);
        trackPlayer = true;
    }

    // Animation Event
    public void A1_EndJump()
    {
        JumpEnd(atk1FallForce);
        trackPlayer = false;
    }

    // Animation Event
    public void A1_Finish()
    {
        ai.SetPhysicsMode(false);
        attack1Particles?.Play();
        trackPlayer = false;

        // Play Attack1 sound
        if (audioSource != null && attackBounceSound != null)
        {
            audioSource.PlayOneShot(attackBounceSound);
        }
    }

    private void Attack1OnHit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerHealth?.TakeDamage(atk1Damage);

        other.GetComponent<PlayerMovement>()?.ApplyKnockback(
            (other.transform.position - transform.position).normalized,
            atk1Knockback
        );
    }

    // ================================
    // ATTACK 2 (Animation Based)
    // ================================

    public void StartAttack2()
    {
        bounceCounter = 0;
        isFinalSmash = false;
        animator.SetTrigger("Attack2");
    }

    // Animation Event: Jump Up
    public void A2_StartBounceJump()
    {
        if (bounceCounter < maxBounces)
        {
            trackPlayer = false;
            JumpStart(atk2JumpForce);
        }
        else
        {
            // Final Smash jump
            isFinalSmash = true;
            trackPlayer = true;
            JumpStart(atk2JumpForce);
        }
    }

    // Animation Event: Hold
    public void A2_Hold()
    {
        float targetHeight = isFinalSmash ? atk2SmashHeight : atk2BounceHeight;
        JumpHold(targetHeight);
        trackPlayer = true;
    }

    // Animation Event: Fall
    public void A2_Fall()
    {
        float fallSpeed = isFinalSmash ? atk2SmashFallForce : atk2FallForce;
        JumpEnd(fallSpeed);
        trackPlayer = false;
    }

    // Animation Event: Ground Impact
    public void A2_Impact()
    {
        if (isFinalSmash)
        {
            smashParticles?.Play();

            // Play final smash sound
            if (audioSource != null && finalSmashSound != null)
                audioSource.PlayOneShot(finalSmashSound);
        }
        else
        {
            bounceCounter++;

            // Play bounce sound
            if (audioSource != null && attackBounceSound != null)
                audioSource.PlayOneShot(attackBounceSound);
        }

        trackPlayer = false;
    }


    // Animation Event: Attack2 Finished
    public void A2_End()
    {
        ai.SetPhysicsMode(false);
        isFinalSmash = false;
        trackPlayer = false;
    }

    private void Attack2BounceOnHit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerHealth?.TakeDamage(atk2BounceDamage);
    }

    private void Attack2SmashOnHit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerHealth?.TakeDamage(atk2SmashDamage);
        other.GetComponent<PlayerMovement>()?.ApplyKnockback(
            (other.transform.position - transform.position).normalized,
            atk2SmashKnockback
        );
    }

    // =====================
    // Shared Jump Logic
    // =====================
    private void JumpStart(float force)
    {
        ai.SetPhysicsMode(true);
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, force, rb.linearVelocity.z); // apply upward force
    }

    private void JumpHold(float height)
    {
        // Freeze physics
        rb.isKinematic = true;
        rb.useGravity = false;

        // ZERO out all momentum and forces
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Force internal physics state to sync with transform
        rb.position = rb.position;

        // Snap to hold height
        Vector3 p = transform.position;
        p.y = height;
        transform.position = p;
    }

    private void JumpEnd(float downwardForce)
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        // Apply controlled downward force
        rb.linearVelocity = new Vector3(0, -downwardForce, 0);
        rb.angularVelocity = Vector3.zero;
    }

    private void MoveAbovePlayer(float height)
    {
        Vector3 pos = player.position;
        pos.y = height;
        transform.position = pos;
    }
}
