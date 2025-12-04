using UnityEngine;

public class SlashLifetime : MonoBehaviour
{
    [Header("Timing")]
    public float lifetime = 0.25f;

    [Header("Swing")]
    [Tooltip("Base sweep angle in degrees (used for the left side and base right side).")]
    public float swingAngle = 160f;

    [Tooltip("How much further to go to the right, relative to the left side.\n" +
             "1 = symmetric, >1 = further right.")]
    public float rightSideMultiplier = 1.2f;

    [Header("Fade")]
    [Tooltip("If true, the slash fades/thins out over its lifetime.")]
    public bool fadeOverTime = true;

    private float elapsed;

    // World-space pivot + rotation at attack start
    private Vector3 pivotPosition;
    private Quaternion baseWorldRotation;

    // Trail / material stuff for fading
    private TrailRenderer trail;
    private Material trailMat;
    private Color baseColor;
    private float initialWidthMultiplier = 1f;

    void Start()
    {
        pivotPosition = transform.position;
        baseWorldRotation = transform.rotation;

        transform.SetParent(null, true);

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

        float startAngle = -swingAngle * 0.5f;
        transform.position = pivotPosition;
        transform.rotation = baseWorldRotation * Quaternion.Euler(0f, startAngle, 0f);

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        float startAngle = -swingAngle * 0.5f;
        float endAngle   = swingAngle * 0.5f * rightSideMultiplier;
        float angle      = Mathf.Lerp(startAngle, endAngle, t);

        transform.position = pivotPosition; 
        transform.rotation = baseWorldRotation * Quaternion.Euler(0f, angle, 0f);

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
