using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // 拖入敌人预制体
    public Transform player;       // 拖入 PlayerBody
    public float spawnRadius = 20f;
    public float spawnInterval = 2f;

    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnEnemy();
            timer = 0f;
        }
    }

    void SpawnEnemy()
    {
        if (!enemyPrefab || !player) return;

        // 在玩家周围随机生成敌人（XZ 平面）
        Vector2 rand = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 spawnPos = new Vector3(
            player.position.x + rand.x,
            player.position.y,
            player.position.z + rand.y
        );

        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }
}
