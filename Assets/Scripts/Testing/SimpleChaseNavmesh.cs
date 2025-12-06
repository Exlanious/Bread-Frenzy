using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleChaseNavmesh : MonoBehaviour
{
    public Transform target;

    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                target = p.transform;
        }
    }

    void Start()
    {
        if (agent == null) return;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
            Debug.Log($"{name}: snapped to NavMesh at {hit.position}");
        }
        else
        {
            Debug.LogWarning($"{name}: no NavMesh within 5 units of {transform.position}");
        }
    }

    void Update()
    {
        if (agent == null || !agent.enabled) return;
        if (!agent.isOnNavMesh) return;
        if (target == null) return;

        agent.speed = 3.5f;      // tweak as needed
        agent.stoppingDistance = 0.5f;

        agent.SetDestination(target.position);
    }
}
