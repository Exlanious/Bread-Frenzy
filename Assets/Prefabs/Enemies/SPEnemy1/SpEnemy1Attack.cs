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

    [Header("Facing")]
    public bool facePlayer = true;
    public float turnSpeed = 720f;

    [Header("Targeting")]
    public bool lockLandingOnHold = true;
    public float landingXZLockSpeed = 60f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip attackBounceSound;
    public AudioClip finalSmashSound;

    [Header("Attack 1 Settings")]
    public CollisionBroadcaster attack1Hitbox;
    public ParticleSystem attack1Particles;
    public float atk1JumpHeight = 10f;
    public float atk1JumpForce = 8f;
    public float atk1FallForce = 10f;
    public int atk1Damage = 1;
    public float atk1Knockback = 5f;

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
    [Header("Attack 2 Follow")]
    public float playerMoveThreshold = 0.4f;
    public float followStrength = 0.6f;
    public float followMaxStepPerSecond = 6f;

    private int bounceCounter = 0;
    private const int maxBounces = 3;
    private bool isFinalSmash = false;

    private bool trackPlayer = false;
    private bool hasLockedLanding = false;
    private Vector3 landingPos;
    private Vector3 lastPlayerPos;
    private Vector3 playerVel;

    private void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody>();
        ai = GetComponent<EnemyMoveAI>();

        if (animator == null)
            animator = GetComponent<Animator>();

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

        if (player != null)
        {
            playerVel = (player.position - lastPlayerPos) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPlayerPos = player.position;
        }

        FacePlayer();

        if (trackPlayer)
            MoveAbovePlayer(transform.position.y);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void InitializeTarget(Transform playerT, PlayerHealth health)
    {
        if (playerT != null) player = playerT;
        if (health != null) playerHealth = health;
    }

    private IEnumerator RandomAttackRoutine()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            int choice = Random.Range(0, 2);

            if (choice == 0)
                StartAttack1();
            else
                StartAttack2();

            yield return new WaitForSeconds(attackInterval);
        }
    }

    private void FacePlayer()
    {
        if (!facePlayer || player == null) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        if (toPlayer.sqrMagnitude < 0.0001f) return;

        Quaternion target = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
    }

    public void StartAttack1()
    {
        hasLockedLanding = false;
        trackPlayer = false;
        animator.SetTrigger("Attack1");
    }

    public void A1_StartJump()
    {
        JumpStart(atk1JumpForce);
        trackPlayer = false;
    }

    public void A1_HoldHeight()
    {
        if (!hasLockedLanding)
        {
            LockLandingToPlayer();
            hasLockedLanding = true;
        }

        JumpHold(atk1JumpHeight);

        if (!lockLandingOnHold)
        {
            trackPlayer = true;
            hasLockedLanding = false;
        }
        else
        {
            trackPlayer = true;
        }
    }

    public void A1_EndJump()
    {
        JumpEnd(atk1FallForce);
        trackPlayer = false;
    }

    public void A1_Finish()
    {
        ai.SetPhysicsMode(false);
        attack1Particles?.Play();
        trackPlayer = false;

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

    public void StartAttack2()
    {
        bounceCounter = 0;
        isFinalSmash = false;
        hasLockedLanding = false;
        trackPlayer = false;
        animator.SetTrigger("Attack2");
    }

    public void A2_StartBounceJump()
    {
        trackPlayer = false;

        if (bounceCounter < maxBounces)
        {
            isFinalSmash = false;
        }
        else
        {
            isFinalSmash = true;
        }

        JumpStart(atk2JumpForce);
    }

    public void A2_Hold()
    {
        if (!hasLockedLanding)
        {
            LockLandingToPlayer();
            hasLockedLanding = true;

            if (player != null) lastPlayerPos = player.position;
        }

        if (player != null && !isFinalSmash && lockLandingOnHold && hasLockedLanding)
        {
            playerVel = (player.position - lastPlayerPos) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPlayerPos = player.position;

            float speed = new Vector2(playerVel.x, playerVel.z).magnitude;
            if (speed > playerMoveThreshold)
            {
                Vector3 desired = player.position;
                desired.y = landingPos.y;

                Vector3 blended = Vector3.Lerp(landingPos, desired, followStrength);
                landingPos = Vector3.MoveTowards(landingPos, blended, followMaxStepPerSecond * Time.deltaTime);
            }
        }

        float targetHeight = isFinalSmash ? atk2SmashHeight : atk2BounceHeight;
        JumpHold(targetHeight);

        if (!lockLandingOnHold)
        {
            trackPlayer = true;
            hasLockedLanding = false;
        }
        else
        {
            trackPlayer = true;
        }
    }

    public void A2_Fall()
    {
        float fallSpeed = isFinalSmash ? atk2SmashFallForce : atk2FallForce;
        JumpEnd(fallSpeed);
        trackPlayer = false;
    }

    public void A2_Impact()
    {
        if (isFinalSmash)
        {
            smashParticles?.Play();

            if (audioSource != null && finalSmashSound != null)
                audioSource.PlayOneShot(finalSmashSound);
        }
        else
        {
            bounceCounter++;

            if (audioSource != null && attackBounceSound != null)
                audioSource.PlayOneShot(attackBounceSound);
        }

        trackPlayer = false;
    }

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

    private void JumpStart(float force)
    {
        ai.SetPhysicsMode(true);
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, force, rb.linearVelocity.z);
    }

    private void JumpHold(float height)
    {
        rb.isKinematic = true;
        rb.useGravity = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = rb.position;

        Vector3 p = transform.position;
        p.y = height;
        transform.position = p;
    }

    private void JumpEnd(float downwardForce)
    {
        rb.isKinematic = false;
        rb.useGravity = true;

        rb.linearVelocity = new Vector3(0, -downwardForce, 0);
        rb.angularVelocity = Vector3.zero;
    }

    private void LockLandingToPlayer()
    {
        if (player == null) return;

        landingPos = player.position;
        landingPos.y = transform.position.y;
    }

    private void MoveAbovePlayer(float height)
    {
        if (player == null) return;

        Vector3 targetXZ;
        if (lockLandingOnHold && hasLockedLanding)
        {
            targetXZ = landingPos;
        }
        else
        {
            targetXZ = player.position;
        }

        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, targetXZ.x, landingXZLockSpeed * Time.deltaTime);
        pos.z = Mathf.MoveTowards(pos.z, targetXZ.z, landingXZLockSpeed * Time.deltaTime);
        pos.y = height;

        transform.position = pos;
    }
}
