using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float lifetime = 3f;
    public int damage = 1;
    public LayerMask hittableLayers = ~0;

    private Transform owner;

    public void Initialize(Transform owner, int damage, float speed, float lifetime, LayerMask hittableLayers)
    {
        this.owner = owner;
        this.damage = damage;
        this.speed = speed;
        this.lifetime = lifetime;
        this.hittableLayers = hittableLayers;

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null) return;
        if (owner != null && other.transform == owner) return;

        if (((1 << other.gameObject.layer) & hittableLayers) == 0) return;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            Vector3 dir = (other.transform.position - transform.position);
            dir.y = 0f;
            dir = dir.normalized;

            enemyHealth.TakeDamage(damage, dir);
            Destroy(gameObject);
        }
    }
}
