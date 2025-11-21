

using UnityEngine;

public class KillPlayerArea : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"💀 Player entered kill zone: {name}");
        var health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(99999); // overkill, literally
        }
    }

}

