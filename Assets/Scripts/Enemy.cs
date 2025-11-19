using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 3f;      // 追踪速度
    public float stopDistance = 1.5f; // 离玩家多近时停止（避免重叠）
    private Transform player;

    void Start()
    {
   
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("playertagmissing");
        }
    }

    void Update()
    {
        if (!player) return;

        // 计算方向与距离
        Vector3 dir = (player.position - transform.position);
        float dist = dir.magnitude;
        dir.Normalize();

        // 转向玩家
        if (dist > stopDistance)
        {
            transform.position += dir * moveSpeed * Time.deltaTime;
        }

        // 让敌人始终面向玩家
        Vector3 lookDir = new Vector3(dir.x, 0, dir.z);
        if (lookDir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 10f * Time.deltaTime);
    }
}
