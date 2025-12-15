using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Rigidbody))]
public class EnemyMoveAI : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Movement")]
    public bool moveByAgent = true;
    public float chaseSpeed = 3.5f;
    public float stopDistance = 1.5f;
    public float navMeshSnapDistance = 50f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;
    public bool isGrounded;

    [Header("Duck Ambient")]
    public AudioSource audioSource;
    public AudioClip duckAmbient;
    public float ambientMinDelay = 3f;
    public float ambientMaxDelay = 7f;
    public float pitchVariation = 0.15f;

    [Header("Stuck Recovery")]
    public bool enableStuckRecovery = true;
    public LayerMask stuckWorldMask = ~0;
    public float stuckMoveEpsilon = 0.03f;
    public float stuckTimeToRecover = 0.6f;
    public float recoverCooldown = 0.4f;
    public int maxPushIterations = 6;
    public float pushExtra = 0.02f;

    float ambientTimer;

    NavMeshAgent agent;
    Rigidbody rb;
    Collider selfCol;

    bool loggedStartState;
    bool loggedFirstDestination;

    Vector3 _lastPos;
    float _stuckTimer;
    float _recoverCooldownTimer;
    Vector3 _lastDest;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        selfCol = GetComponent<Collider>();

        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        ambientTimer = Random.Range(ambientMinDelay, ambientMaxDelay);
    }

    void Start()
    {
        SnapToNavMesh();
        LogStartState();

        _lastPos = transform.position;
        _stuckTimer = 0f;
        _recoverCooldownTimer = 0f;
        _lastDest = transform.position;
    }

    void OnEnable()
    {
        SnapToNavMesh();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;

        _lastPos = transform.position;
        _stuckTimer = 0f;
        _recoverCooldownTimer = 0f;
        _lastDest = transform.position;
    }

    void OnDisable()
    {
        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;
    }

    void Update()
    {
        HandleAmbientSound();

        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (!moveByAgent)
            return;

        if (agent == null)
        {
            Debug.LogError($"{name}: EnemyMoveAI - NavMeshAgent missing.");
            return;
        }

        if (!agent.enabled)
        {
            Debug.LogWarning($"{name}: EnemyMoveAI - agent disabled.");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            SnapToNavMesh();
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning($"{name}: EnemyMoveAI - still not on NavMesh after snap.");
                return;
            }
        }

        if (player == null)
        {
            Debug.LogWarning($"{name}: EnemyMoveAI - player is null.");
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

        if (dist <= stopDistance)
        {
            agent.isStopped = true;
            _stuckTimer = 0f;
            _lastPos = transform.position;
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        Vector3 dest = player.position;
        _lastDest = dest;
        bool ok = agent.SetDestination(dest);

        if (!loggedFirstDestination)
        {
            loggedFirstDestination = true;
            Debug.Log($"{name}: EnemyMoveAI - SetDestination({dest}) success={ok}, isOnNavMesh={agent.isOnNavMesh}, hasPath={agent.hasPath}");
        }

        if (enableStuckRecovery)
            StuckRecoveryTick();
        else
            _lastPos = transform.position;
    }

    void StuckRecoveryTick()
    {
        if (_recoverCooldownTimer > 0f)
        {
            _recoverCooldownTimer -= Time.deltaTime;
            _lastPos = transform.position;
            return;
        }

        if (agent.isStopped || agent.pathPending)
        {
            _stuckTimer = 0f;
            _lastPos = transform.position;
            return;
        }

        Vector3 cur = transform.position;
        float moved = Vector3.Distance(cur, _lastPos);

        bool wantsToMove = agent.desiredVelocity.sqrMagnitude > 0.05f;
        bool actuallyMoving = agent.velocity.sqrMagnitude > 0.01f;

        if (wantsToMove && !actuallyMoving && moved <= stuckMoveEpsilon)
        {
            _stuckTimer += Time.deltaTime;
            if (_stuckTimer >= stuckTimeToRecover)
            {
                bool freed = PushOutOfColliders();
                SnapToNavMesh();

                if (agent.isOnNavMesh)
                {
                    agent.ResetPath();
                    agent.SetDestination(_lastDest);
                }

                _recoverCooldownTimer = recoverCooldown;
                _stuckTimer = 0f;

                if (!freed)
                {
                    // still do cooldown + path reset; nothing else needed
                }
            }
        }
        else
        {
            _stuckTimer = 0f;
        }

        _lastPos = cur;
    }

    bool PushOutOfColliders()
    {
        if (selfCol == null)
            selfCol = GetComponent<Collider>();

        if (selfCol == null)
            return false;

        bool anyResolved = false;

        for (int iter = 0; iter < Mathf.Max(1, maxPushIterations); iter++)
        {
            Bounds b = selfCol.bounds;
            Vector3 center = b.center;
            Vector3 halfExtents = b.extents;

            Collider[] hits = Physics.OverlapBox(center, halfExtents, transform.rotation, stuckWorldMask, QueryTriggerInteraction.Ignore);
            bool resolvedThisIter = false;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider other = hits[i];
                if (other == null) continue;
                if (other == selfCol) continue;
                if (!other.enabled) continue;
                if (other.isTrigger) continue;

                Vector3 dir;
                float dist;
                bool overlapped = Physics.ComputePenetration(
                    selfCol, transform.position, transform.rotation,
                    other, other.transform.position, other.transform.rotation,
                    out dir, out dist
                );

                if (overlapped && dist > 0f)
                {
                    transform.position += dir * (dist + pushExtra);
                    resolvedThisIter = true;
                    anyResolved = true;
                }
            }

            if (!resolvedThisIter)
                break;
        }

        return anyResolved;
    }

    void HandleAmbientSound()
    {
        if (duckAmbient == null) return;

        ambientTimer -= Time.deltaTime;

        if (ambientTimer <= 0f)
        {
            if (audioSource != null)
            {
                audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
                audioSource.PlayOneShot(duckAmbient);
            }
            ambientTimer = Random.Range(ambientMinDelay, ambientMaxDelay);
        }
    }

    void SnapToNavMesh()
    {
        if (agent == null) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, navMeshSnapDistance, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            Debug.LogWarning($"{name}: EnemyMoveAI - no NavMesh within {navMeshSnapDistance} of {transform.position}");
        }
    }

    public void SetPhysicsMode(bool usePhysics)
    {
        if (agent == null || rb == null) return;

        if (usePhysics)
        {
            if (agent.enabled && agent.isOnNavMesh)
                agent.isStopped = true;

            agent.enabled = false;
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        else
        {
            rb.isKinematic = true;
            rb.useGravity = false;

            agent.enabled = true;
            SnapToNavMesh();

            if (agent.isOnNavMesh)
            {
                agent.ResetPath();
                agent.isStopped = false;
            }
        }
    }

    void LogStartState()
    {
        if (loggedStartState) return;
        loggedStartState = true;

        string playerName = player != null ? player.name : "null";
        bool hasAgent = agent != null;
        bool onMesh = hasAgent && agent.isOnNavMesh;
        float speed = hasAgent ? agent.speed : 0f;

        Debug.Log($"{name}: EnemyMoveAI Start - moveByAgent={moveByAgent}, hasAgent={hasAgent}, enabled={hasAgent && agent.enabled}, isOnNavMesh={onMesh}, agentSpeed={speed}, player={playerName}");
    }

    void OnDrawGizmosSelected()
    {
        if (agent != null)
        {
            Gizmos.color = agent.enabled ? Color.yellow : Color.red;
            Gizmos.DrawWireSphere(transform.position, stopDistance);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, navMeshSnapDistance);
    }
}
