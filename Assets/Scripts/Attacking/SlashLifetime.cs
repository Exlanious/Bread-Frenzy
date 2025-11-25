using UnityEngine;

public class SlashLifetime : MonoBehaviour
{
    public float lifetime = 0.2f;
    public float swingAngle = 160f;

    private float elapsed;
    private Quaternion baseLocalRotation;

    void Start()
    {
        baseLocalRotation = transform.localRotation;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        float angle = Mathf.Lerp(-swingAngle * 0.5f, swingAngle * 0.5f, t);

        // rotate relative to the player (parent)
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, angle, 0f);
    }
}
