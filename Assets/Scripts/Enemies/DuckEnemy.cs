using UnityEngine;

public class DuckEnemy : MonoBehaviour
{
    [HideInInspector] public EnemyHordeSpawner spawner;

    // later, when you actually kill the duck:
    void Die()
    {
        if (spawner != null)
            spawner.NotifyEnemyDied();

        Destroy(gameObject);
    }
}
