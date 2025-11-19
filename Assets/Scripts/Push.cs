using UnityEngine;

public class PlayerPushAbility : MonoBehaviour
{
    [Header("Push Settings")]
    [Tooltip("Force")]
    public float pushForce = 15f;

    [Tooltip("Radius")]
    public float pushRadius = 8f;

    [Tooltip("CD/sec")]
    public float cooldownTime = 3f;

    [Tooltip("Tag")]
    public string enemyTag = "Enemy";

    [Header("VFX")]
    [Tooltip("释放时播放的粒子特效")]
    public ParticleSystem pushEffect;
    [Tooltip("Sound")]
    public AudioSource pushSound;

    private float nextAvailableTime = 0f;  // 下次可用时间

    void Update()
    {
        // 检查是否按下空格 且冷却完毕
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextAvailableTime)
        {
            DoPush();
        }
    }

    void DoPush()
    {
        nextAvailableTime = Time.time + cooldownTime;

        // 播放视觉/音效反馈
        if (pushEffect != null)
        {
            pushEffect.transform.position = transform.position + Vector3.up * 1f;
            pushEffect.Play();
        }

        if (pushSound != null)
            pushSound.Play();

        // 检测范围内所有碰撞体
        Collider[] hits = Physics.OverlapSphere(transform.position, pushRadius);
        foreach (Collider col in hits)
        {
            if (col.CompareTag(enemyTag))
            {
                Rigidbody rb = col.attachedRigidbody;
                if (rb != null)
                {
                    // 计算推开方向
                    Vector3 dir = (col.transform.position - transform.position).normalized;
                    rb.AddForce(dir * pushForce, ForceMode.Impulse);
                }
                else
                {
                    // 如果敌人没有刚体，用位置推开
                    Vector3 dir = (col.transform.position - transform.position).normalized;
                    col.transform.position += dir * (pushForce * 0.2f);
                }
            }
        }

        Debug.Log("Player Push Activated!");
    }

    // 在Scene视图中绘制范围圈
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, pushRadius);
    }
}
