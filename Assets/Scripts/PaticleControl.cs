using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerHitParticle : MonoBehaviour
{
    [Header("References")]
    public ParticleSystem hitParticle;   // 掉落碎屑或飞散特效
    public string enemyTag = "Enemy";    // 敌人标签
    public float particleBurstRate = 30f; // 接触时粒子生成速率
    public float normalRate = 0f;        // 不接触时粒子速率
    public float burstSpread = 1.5f;     // 粒子喷射强度（视觉用）

    private bool isTouchingEnemy = false;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.ShapeModule shape;

    void Start()
    {
        if (!hitParticle)
        {
            Debug.LogWarning("未指定粒子系统 hitParticle！");
            return;
        }

        emission = hitParticle.emission;
        shape = hitParticle.shape;

        // 默认关闭播放
        emission.rateOverTime = normalRate;
        hitParticle.Stop();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            StartParticle(other.transform.position);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            StopParticle();
        }
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.collider.CompareTag(enemyTag))
        {
            StartParticle(hit.point);
        }
        else
        {
            StopParticle();
        }
    }

    void StartParticle(Vector3 hitPoint)
    {
        if (!hitParticle) return;

        isTouchingEnemy = true;

        // 调整粒子方向：从玩家朝外喷发
        Vector3 dir = (transform.position - hitPoint).normalized;
        shape.angle = 25f;
        shape.rotation = Quaternion.LookRotation(dir).eulerAngles;

        // 开始播放粒子
        emission.rateOverTime = particleBurstRate;
        if (!hitParticle.isPlaying)
            hitParticle.Play();
    }

    void StopParticle()
    {
        if (!hitParticle) return;
        if (!isTouchingEnemy) return;

        isTouchingEnemy = false;

        // 停止发射粒子
        emission.rateOverTime = normalRate;
        hitParticle.Stop();
    }
}
