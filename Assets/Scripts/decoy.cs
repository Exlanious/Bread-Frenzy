using UnityEngine;

public class PlayerDecoyAbility : MonoBehaviour
{
    [Header("Decoy Settings")]
    [Tooltip("Prefab of the Decoy (must include Rigidbody + Decoy.cs).")]
    public GameObject decoyPrefab;

    [Tooltip("Force applied forward when shooting the decoy.")]
    public float shootForce = 15f;

    [Tooltip("Extra upward force for arc motion.")]
    public float upwardForce = 3f;

    [Tooltip("Cooldown time between shots (seconds).")]
    public float cooldownTime = 8f;

    [Tooltip("Optional visual effect when firing.")]
    public ParticleSystem spawnEffect;

    private float nextAvailableTime = 0f;

    void Update()
    {
        // Press F to shoot if cooldown is over
        if (Input.GetKeyDown(KeyCode.F) && Time.time >= nextAvailableTime)
        {
            ShootDecoy();
        }
    }

    void ShootDecoy()
    {
        nextAvailableTime = Time.time + cooldownTime;

        // Spawn position slightly in front of player
        Vector3 spawnPos = transform.position + transform.forward * 1.5f + Vector3.up * 1f;
        GameObject decoy = Instantiate(decoyPrefab, spawnPos, Quaternion.identity);

        // Optional spawn effect
        if (spawnEffect != null)
            Instantiate(spawnEffect, spawnPos, Quaternion.identity);

        // Make sure decoy has a Rigidbody
        Rigidbody rb = decoy.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Reset velocity to prevent strange behavior
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            // Compute launch direction
            Vector3 shootDir = (transform.forward + Vector3.up * 0.15f).normalized;

            // Apply forward + upward impulse
            rb.AddForce(shootDir * shootForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Decoy prefab has no Rigidbody!");
        }

        Debug.Log("Decoy launched!");
    }
}
