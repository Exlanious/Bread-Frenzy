using UnityEngine;

public class SlashLifetime : MonoBehaviour
{
    public float lifetime = 0.25f;
    public float startAngle = -110f;
    public float endAngle = 0f;
    public bool fadeOverTime = true;

    float elapsed;

    Vector3 localPivotPosition;
    Quaternion baseLocalRotation;

    TrailRenderer trail;
    Material trailMat;
    Color baseColor;
    float initialWidthMultiplier = 1f;

    void Start()
    {
        localPivotPosition = transform.localPosition;
        baseLocalRotation  = transform.localRotation;

        trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            trail.material = new Material(trail.material);
            trailMat = trail.material;

            initialWidthMultiplier = trail.widthMultiplier;

            if (trailMat.HasProperty("_BaseColor"))
                baseColor = trailMat.GetColor("_BaseColor");
            else if (trailMat.HasProperty("_Color"))
                baseColor = trailMat.GetColor("_Color");
            else
                baseColor = Color.white;

            trail.Clear();
        }

        transform.localPosition = localPivotPosition;
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, startAngle, 0f);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        float angle = Mathf.Lerp(startAngle, endAngle, t);

        transform.localPosition = localPivotPosition;
        transform.localRotation = baseLocalRotation * Quaternion.Euler(0f, angle, 0f);

        if (fadeOverTime && trail != null && trailMat != null)
        {
            float fade = 1f - t;

            if (trailMat.HasProperty("_BaseColor"))
            {
                Color c = baseColor;
                c.a *= fade;
                trailMat.SetColor("_BaseColor", c);
            }
            else if (trailMat.HasProperty("_Color"))
            {
                Color c = baseColor;
                c.a *= fade;
                trailMat.SetColor("_Color", c);
            }

            trail.widthMultiplier = Mathf.Lerp(initialWidthMultiplier,
                                               initialWidthMultiplier * 0.2f,
                                               t);
        }
    }
}
