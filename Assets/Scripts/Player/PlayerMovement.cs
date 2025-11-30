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
    public float dashDistance = 4f;      // NEW: how far the dash travels
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.4f;

    private bool isDashing;
    private bool canDash = true;

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 currentVelocity;
    private Vector3 velocitySmoothRef;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true; // prevent tipping
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

        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isDashing)
            return;

        // Input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Camera-relative direction
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 inputDir = (forward * z + right * x).normalized;
        Vector3 targetVelocity = inputDir * moveSpeed;

        // Preserve Y velocity
        Vector3 currentVel = rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(currentVel.x, 0, currentVel.z);

        // Smooth acceleration & deceleration
        horizontalVel = Vector3.SmoothDamp(
            horizontalVel,
            targetVelocity,
            ref velocitySmoothRef,
            (inputDir.magnitude > 0.1f) ? (1f / acceleration) : (1f / deceleration)
        );

        // Combine smoothed horizontal + existing vertical
        rb.linearVelocity = new Vector3(horizontalVel.x, currentVel.y, horizontalVel.z);

        // Rotate toward movement direction
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
            // Reset vertical velocity before jump to ensure consistent height
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

        // Direction = where the player is facing, horizontal only
        Vector3 dashDir = transform.forward;
        dashDir.y = 0f;
        dashDir.Normalize();

        float elapsed = 0f;

        Vector3 startPos = rb.position;
        Vector3 targetPos = startPos + dashDir * dashDistance;

        // Optional: reduce drag during dash so it feels clean
        float originalDrag = rb.linearDamping;
        rb.linearDamping = 0f;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;

            // Smooth ease (fast at start, slow at end)
            float eased = Mathf.SmoothStep(0f, 1f, t);

            Vector3 newPos = Vector3.Lerp(startPos, targetPos, eased);

            // Keep current vertical position (so jump / gravity still work)
            newPos.y = rb.position.y;

            rb.MovePosition(newPos);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Snap to final target to avoid tiny drift
        Vector3 finalPos = targetPos;
        finalPos.y = rb.position.y;
        rb.MovePosition(finalPos);

        rb.linearDamping = originalDrag;

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }


    //Knockback -> set direction y to 0 for no y knockback
    public void ApplyKnockback(Vector3 direction, float force)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.AddForce(direction.normalized * force, ForceMode.Impulse);
    }

}
