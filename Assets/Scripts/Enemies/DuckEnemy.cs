using UnityEngine;

public class DuckEnemy : MonoBehaviour
{
    [HideInInspector] public EnemyHordeSpawner spawner;

    void Die()
    {
        if (spawner != null)
            spawner.NotifyEnemyDied();

        Destroy(gameObject);
    }
}
