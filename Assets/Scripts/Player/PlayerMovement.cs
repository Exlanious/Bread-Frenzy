using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 6f;
    public float acceleration = 10f;  // higher = faster response
    public float deceleration = 10f;
    public float jumpForce = 7f;

    [Header("References")]
    public Transform cameraTransform;
    public Transform groundCheck;

    [Header("Ground Check")]
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    [Header("Dash Settings")]
    public KeyCode dashKey = KeyCode.LeftShift;
    // public float dashSpeed = 15f; 
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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (playerStats == null)
            playerStats = GetComponent<PlayerStats>();
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
        HandleMovement();
        HandleJump();
        HandleDash();
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(DashRoutine());
        }
    }

    void HandleMovement()
    {

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isDashing)
            return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 inputDir = (forward * z + right * x).normalized;
        Vector3 targetVelocity = inputDir * moveSpeed;

        Vector3 currentVel = rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);

        horizontalVel = Vector3.SmoothDamp(
            horizontalVel,
            targetVelocity,
            ref velocitySmoothRef,
            (inputDir.magnitude > 0.1f) ? (1f / acceleration) : (1f / deceleration)
        );

        rb.linearVelocity = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);

        if (inputDir.magnitude > 0.1f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = 0f;
            rb.linearVelocity = vel;

            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
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
        dashDir.Normalize();

        float elapsed = 0f;

        Vector3 startPos = rb.position;
        Vector3 targetPos = startPos + dashDir * dashDistance;

        float originalDrag = rb.linearDamping;
        rb.linearDamping = 0f;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;

            float eased = Mathf.SmoothStep(0f, 1f, t);

            Vector3 newPos = Vector3.Lerp(startPos, targetPos, eased);

            newPos.y = rb.position.y;

            rb.MovePosition(newPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 finalPos = targetPos;
        finalPos.y = rb.position.y;
        rb.MovePosition(finalPos);

        rb.linearDamping = originalDrag;

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
        dir = dir.normalized;

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
