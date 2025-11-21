using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class EnemyMoveAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement Settings")]
    public bool moveByAgent = false;
    public float chaseSpeed = 3.5f;
    public float stopDistance = 1.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    public bool isGrounded = false;

    [Header("Teleport/Stun Settings")]
    [SerializeField] private float teleportHeightForce = 5f;
    [SerializeField] private float stunDuration = 1f;

    [Header("NavMesh Settings")]
    [Tooltip("Maximum distance to snap to nearest NavMesh position after teleport.")]
    [SerializeField] private float navMeshSnapDistance = 2f;

    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rigidBody;


    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (rigidBody == null) rigidBody = GetComponent<Rigidbody>();

        agent.stoppingDistance = 0f;          // we handle distance manually
    }

    void Update()
    {
        // Ground check
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        // Safety: stop all movement logic if not ready
        if (player == null || !moveByAgent || agent == null)
            return;

        if (!agent.enabled || !agent.isOnNavMesh)
            return;

        // Direction to player on XZ plane
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float distance = toPlayer.magnitude;

        // If already within stop distance, just stop the agent
        if (distance <= stopDistance)
        {
            agent.isStopped = true;
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        // Target a point at "stopDistance" away from the player
        Vector3 targetPos = player.position - toPlayer.normalized * stopDistance;
        agent.SetDestination(targetPos);
    }

    void OnDisable()
    {
        if (agent.isActiveAndEnabled)
            agent.isStopped = true;
    }

    void OnEnable()
    {
        if (agent.isActiveAndEnabled)
            agent.isStopped = false;
    }

    // --------------------------------------------------------------------
    // TELEPORT -> Bread Frenzy: Not implemented. 
    // --------------------------------------------------------------------
    public void Teleport(Vector3 targetPosition)
    {
        StartCoroutine(TeleportResolveCoroutine(targetPosition));
    }

    private IEnumerator TeleportResolveCoroutine(Vector3 targetPosition)
    {
        yield return null; // Wait one frame to ensure transform sync

        // Temporarily disable agent
        agent.enabled = false;

        // --- Try snap to nearest NavMesh ---
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            transform.position = hit.position;
        }
        else
        {
            // --- Set up physics state ---
            rigidBody.useGravity = true;
            rigidBody.isKinematic = false;
            rigidBody.linearVelocity = Vector3.up * teleportHeightForce;

            // Teleport
            transform.position = targetPosition;
            Physics.SyncTransforms();

            // --- Wait for ground detection ---
            isGrounded = false;
            yield return new WaitForFixedUpdate();
            yield return new WaitUntil(() => isGrounded);
            yield return new WaitForSeconds(stunDuration);

            // --- Re-enable NavMesh Agent ---
            rigidBody.linearVelocity = Vector3.zero;
            rigidBody.angularVelocity = Vector3.zero;
            rigidBody.isKinematic = true;
            rigidBody.useGravity = false;

        }
        agent.Warp(transform.position);
        agent.enabled = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled == false) return;
        Gizmos.color = agent != null && agent.enabled ? Color.yellow : Color.red;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        // Draw snap distance radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, navMeshSnapDistance);
    }
}
