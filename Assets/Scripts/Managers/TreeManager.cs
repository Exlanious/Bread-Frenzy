using UnityEngine;
using System.Collections.Generic;

public class TreeManager : MonoBehaviour
{
    [Header("Tree Settings")]
    public GameObject[] treePrefabs;   // 四种树Prefab
    public int maxTrees = 300;         // 最大存在树数量
    public float spawnRadius = 60f;    // 玩家周围生成半径
    public float minTreeDistance = 6f; // 树间最小距离
    public float removeDistance = 90f; // 离玩家超过此距离则销毁
    public float checkInterval = 3f;   // 检测/生成间隔

    [Header("References")]
    public Transform player;
    public LayerMask groundMask;       // 地面层（用于射线检测）

    private List<GameObject> spawnedTrees = new List<GameObject>();

    void Start()
    {
        if (treePrefabs.Length == 0)
        {
            Debug.LogWarning("🌲 TreeManager: 请在 Inspector 中拖入4个树 Prefab！");
            return;
        }

        InvokeRepeating(nameof(UpdateTreesAroundPlayer), 0f, checkInterval);
    }

    void UpdateTreesAroundPlayer()
    {
        if (!player) return;

        RemoveDistantTrees();

        if (spawnedTrees.Count < maxTrees)
        {
            SpawnTreesAroundPlayer();
        }
    }

    void SpawnTreesAroundPlayer()
    {
        // 尝试多次以找到合适位置
        for (int i = 0; i < 15; i++)
        {
            Vector3 randomPos = GetRandomSpawnPosition();

            if (IsTooCloseToOtherTrees(randomPos))
                continue;

            // 检查地面高度
            if (Physics.Raycast(randomPos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f, groundMask))
            {
                GameObject prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                Vector3 finalPos = hit.point;

                // 随机旋转与缩放
                Quaternion rot = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
                float scale = Random.Range(0.9f, 1.3f);

                GameObject newTree = Instantiate(prefab, finalPos, rot);
                newTree.transform.localScale *= scale;

                spawnedTrees.Add(newTree);

                if (spawnedTrees.Count >= maxTrees) break;
            }
        }
    }

    void RemoveDistantTrees()
    {
        for (int i = spawnedTrees.Count - 1; i >= 0; i--)
        {
            GameObject tree = spawnedTrees[i];
            if (tree == null)
            {
                spawnedTrees.RemoveAt(i);
                continue;
            }

            float dist = Vector3.Distance(player.position, tree.transform.position);
            if (dist > removeDistance)
            {
                Destroy(tree);
                spawnedTrees.RemoveAt(i);
            }
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector2 randCircle = Random.insideUnitCircle.normalized * spawnRadius;
        Vector3 pos = new Vector3(
            player.position.x + randCircle.x,
            player.position.y,
            player.position.z + randCircle.y
        );
        return pos;
    }

    bool IsTooCloseToOtherTrees(Vector3 pos)
    {
        foreach (GameObject t in spawnedTrees)
        {
            if (t == null) continue;
            if (Vector3.Distance(pos, t.transform.position) < minTreeDistance)
                return true;
        }
        return false;
    }
}
