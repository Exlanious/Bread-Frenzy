using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [Header("Projectile Stats")]
    public float speed = 25f;     
    public int damage = 1;
    public float lifeTime = 3f;

    [Header("Collision")]
    [Tooltip("What layers should stop this projectile (e.g. Default, Environment). Player is handled by script.")]
    public LayerMask stopOnLayers;

    private Vector3 _direction;
    private bool _initialized;

    public void Initialize(Vector3 direction)
    {
        _direction = direction.normalized;
        _initialized = true;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (!_initialized) return;

        Vector3 delta = _direction * speed * Time.deltaTime;
        transform.position += delta;
    }

    private void OnTriggerEnter(Collider other)
    {
        var playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & stopOnLayers.value) != 0)
        {
            Destroy(gameObject);
        }
    }
}
