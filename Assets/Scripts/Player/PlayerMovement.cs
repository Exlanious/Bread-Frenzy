using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float acceleration = 10f;
    public float deceleration = 10f;
    public float jumpForce = 7f;

    [Header("Game Feel")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float fallGravityMultiplier = 2.5f;
    public float lowJumpGravityMultiplier = 2f;
    public float airControlMultiplier = 0.6f;
    public float inputDeadzone = 0.1f;
    public float rotationSpeed = 10f;

    [Header("References")]
    public Transform cameraTransform;
    public Transform groundCheck;

    [Header("Ground Check")]
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Dash Settings")]
    public KeyCode dashKey = KeyCode.LeftShift;
    public float dashDistance = 4f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.4f;

    [Header("Dash Attack (Crusty Dash)")]
    public AbilityManager abilityManager;
    public AbilityUpgrade crustyDashBase;
    public PlayerStats playerStats;
    public CollisionBroadcaster dashBroadcaster;
    public LayerMask dashHittableLayers;
    public float dashDamageMultiplier = 1.0f;
    public float dashExtraKnockbackMultiplier = 1.5f;
    public float dashStunDuration = 0.2f;

    private bool isDashing;
    private bool canDash = true;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 currentVelocity;
    private Vector3 velocitySmoothRef;

    private float lastGroundedTime;
    private float lastJumpPressedTime;
    private bool jumpHeld;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void OnEnable()
    {
        if (dashBroadcaster != null)
            dashBroadcaster.OnTriggerEnterEvent += OnDashHit;
    }

    void OnDisable()
    {
        if (dashBroadcaster != null)
            dashBroadcaster.OnTriggerEnterEvent -= OnDashHit;
    }

    void Update()
    {
        UpdateGroundedState();
        CacheJumpInput();
        ApplyExtraGravity();

        HandleMovement();
        HandleJump();
        HandleDash();
    }

    void UpdateGroundedState()
    {
        if (groundCheck != null)
        {
            bool groundedNow = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask, QueryTriggerInteraction.Ignore);
            if (groundedNow)
            {
                isGrounded = true;
                lastGroundedTime = Time.time;
            }
            else
            {
                isGrounded = false;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    void CacheJumpInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            lastJumpPressedTime = Time.time;
            jumpHeld = true;
        }

        if (Input.GetButtonUp("Jump"))
        {
            jumpHeld = false;
        }
    }

    void ApplyExtraGravity()
    {
        if (isDashing)
            return;

        Vector3 vel = rb.linearVelocity;

        if (vel.y < 0f)
        {
            vel += Vector3.up * Physics.gravity.y * (fallGravityMultiplier - 1f) * Time.deltaTime;
        }
        else if (vel.y > 0f && !jumpHeld)
        {
            vel += Vector3.up * Physics.gravity.y * (lowJumpGravityMultiplier - 1f) * Time.deltaTime;
        }

        rb.linearVelocity = vel;
    }

    void HandleMovement()
    {
        if (isDashing)
            return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector2 input = new Vector2(x, z);
        if (input.magnitude < inputDeadzone)
        {
            input = Vector2.zero;
        }
        else
        {
            input = input.normalized;
        }

        Vector3 forward = cameraTransform != null ? cameraTransform.forward : Vector3.forward;
        Vector3 right = cameraTransform != null ? cameraTransform.right : Vector3.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 inputDir = (forward * input.y + right * input.x);
        inputDir.Normalize();

        Vector3 currentVel = rb.linearVelocity;
        Vector3 targetHorizontal = inputDir * moveSpeed;

        if (!isGrounded)
            targetHorizontal *= airControlMultiplier;

        Vector3 currentHorizontal = new Vector3(currentVel.x, 0f, currentVel.z);

        float smoothTime = (inputDir.sqrMagnitude > 0.001f) ? (1f / acceleration) : (1f / deceleration);
        Vector3 horizontalVel = Vector3.SmoothDamp(currentHorizontal, targetHorizontal, ref velocitySmoothRef, smoothTime);

        rb.linearVelocity = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);

        if (inputDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }

    void HandleJump()
    {
        if (isDashing)
            return;

        bool canUseCoyote = Time.time - lastGroundedTime <= coyoteTime;
        bool bufferedJump = Time.time - lastJumpPressedTime <= jumpBufferTime;

        if (bufferedJump && canUseCoyote)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            lastJumpPressedTime = -999f;
            lastGroundedTime = -999f;
            isGrounded = false;
        }
    }

    void HandleDash()
    {
        if (isDashing || !canDash)
            return;

        if (Input.GetKeyDown(dashKey))
        {
            StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        canDash = false;
        isDashing = true;

        if (dashBroadcaster != null)
        {
            var col = dashBroadcaster.GetComponent<Collider>();
            if (col != null)
                col.enabled = true;
        }

        Vector3 dashDir = transform.forward;
        dashDir.y = 0f;
        if (dashDir.sqrMagnitude < 0.001f)
            dashDir = transform.forward;
        dashDir.Normalize();

        float dashSpeed = dashDistance / dashDuration;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            Vector3 vel = rb.linearVelocity;
            vel = dashDir * dashSpeed;
            rb.linearVelocity = vel;
            yield return null;
        }

        if (dashBroadcaster != null)
        {
            var col = dashBroadcaster.GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public void ApplyKnockback(Vector3 direction, float force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        direction.y = 0f;
        direction.Normalize();
        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

    private void OnDashHit(Collider other)
    {
        if (!isDashing) return;
        if (other == null) return;
        if (other.gameObject == gameObject) return;

        if (((1 << other.gameObject.layer) & dashHittableLayers) == 0)
            return;

        if (abilityManager == null || crustyDashBase == null)
            return;

        if (!abilityManager.HasUpgrade(crustyDashBase))
            return;

        Vector3 dir = other.transform.position - transform.position;
        dir.y = 0f;
        dir.Normalize();

        int dmg = 1;
        if (playerStats != null)
            dmg = Mathf.Max(1, Mathf.RoundToInt(playerStats.damage * dashDamageMultiplier));

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Vector3 knockDir = dir * dashExtraKnockbackMultiplier;
            enemyHealth.TakeDamage(dmg, knockDir);
        }
    }
}
