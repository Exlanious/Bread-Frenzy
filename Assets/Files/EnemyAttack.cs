using System;
using System.Collections;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Attack Settings")]
    public int damage = 1;
    public float attackCooldown = 0.5f; // seconds between hits
    public float attackRange = 1.5f;

    private float lastAttackTime = -999f;
    private PlayerHealth playerHealth;

    void Awake()
    {

        if (player != null)
            playerHealth = player.GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= attackRange)
        {
            TryAttackPlayer();
        }
    }


    private void TryAttackPlayer()
    {
        if (playerHealth == null) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            playerHealth.TakeDamage(damage);
            lastAttackTime = Time.time;
        }
    }
}