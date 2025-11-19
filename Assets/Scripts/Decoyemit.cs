using UnityEngine;

public class Decoy : MonoBehaviour
{
    [Header("Decoy Behavior")]
    [Tooltip("How long the decoy lasts before disappearing.")]
    public float lifeTime = 6f;

    [Tooltip("Radius of attraction.")]
    public float attractRadius = 15f;

    [Tooltip("Speed enemies move toward the decoy.")]
    public float attractSpeed = 4f;

    [Tooltip("Enemy tag to be affected.")]
    public string enemyTag = "Enemy";

    private float spawnTime;

    void Start()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Destroy after lifetime
        if (Time.time >= spawnTime + lifeTime)
        {
            Destroy(gameObject);
            return;
        }

        // Attract enemies within radius
        Collider[] hits = Physics.OverlapSphere(transform.position, attractRadius);
        foreach (Collider col in hits)
        {
            if (col.CompareTag(enemyTag))
            {
                Transform enemy = col.transform;
                Vector3 dir = (transform.position - enemy.position).normalized;
                enemy.position += dir * attractSpeed * Time.deltaTime;

                // Optional: make enemies face the decoy
                enemy.rotation = Quaternion.Slerp(
                    enemy.rotation,
                    Quaternion.LookRotation(dir),
                    5f * Time.deltaTime
                );
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}
