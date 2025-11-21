using UnityEngine;

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
    }

    void HandleMovement()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

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
}
