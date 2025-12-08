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

    float ambientTimer;



    NavMeshAgent agent;
    Rigidbody rb;

    bool loggedStartState;
    bool loggedFirstDestination;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

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
    }

    void OnEnable()
    {
        SnapToNavMesh();

        if (agent != null && agent.enabled && agent.isOnNavMesh)
            agent.isStopped = false;
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
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        if (!moveByAgent)
        {
            return;
        }

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
            return;
        }

        agent.isStopped = false;
        agent.speed = chaseSpeed;

        Vector3 dest = player.position;
        bool ok = agent.SetDestination(dest);

        if (!loggedFirstDestination)
        {
            loggedFirstDestination = true;
            Debug.Log($"{name}: EnemyMoveAI - SetDestination({dest}) success={ok}, isOnNavMesh={agent.isOnNavMesh}, hasPath={agent.hasPath}");
        }
    }

    void HandleAmbientSound()
    {
        if (duckAmbient == null) return;

        ambientTimer -= Time.deltaTime;

        if (ambientTimer <= 0f)
        {
            // Slight pitch variation
            audioSource.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
            audioSource.PlayOneShot(duckAmbient);

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

    // other scripts call this, so keep it
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
