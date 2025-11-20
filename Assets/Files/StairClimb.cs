using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EasyStep : MonoBehaviour
{
    [Header("Step Settings")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float stepHeight = 0.3f;
    [SerializeField] private float stepSmooth = 2f;
    [SerializeField] private float lowerRayDistance = 0.1f;
    [SerializeField] private float upperRayDistance = 0.2f;

    private Rigidbody rb;


    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        StepClimb();
    }

    private void StepClimb()
    {
        Vector3 originLower = rayOrigin.position + Vector3.up * 0.05f;     // near feet
        Vector3 originUpper = originLower + Vector3.up * stepHeight;       // stepHeight above

        TryStep(originLower, originUpper, transform.forward);
        TryStep(originLower, originUpper, transform.TransformDirection(1.5f, 0, 1));   // +45°
        TryStep(originLower, originUpper, transform.TransformDirection(-1.5f, 0, 1));  // -45°
    }

    private void TryStep(Vector3 lowerOrigin, Vector3 upperOrigin, Vector3 direction)
    {
        // Cast lower ray - detects step base
        if (Physics.Raycast(lowerOrigin, direction, out RaycastHit lowerHit, lowerRayDistance))
        {
            // Cast upper ray - clear path above step
            if (!Physics.Raycast(upperOrigin, direction, upperRayDistance))
            {
                Vector3 newPosition = rb.position + Vector3.up * stepSmooth * Time.fixedDeltaTime;
                rb.MovePosition(newPosition);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (isActiveAndEnabled == false) return;
        if (rayOrigin == null) rayOrigin = transform;

        Vector3 originLower = rayOrigin.position + Vector3.up * 0.05f;
        Vector3 originUpper = originLower + Vector3.up * stepHeight;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(originLower, transform.forward * lowerRayDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(originUpper, transform.forward * upperRayDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(originLower, transform.TransformDirection(1.5f, 0, 1) * lowerRayDistance);
        Gizmos.DrawRay(originLower, transform.TransformDirection(-1.5f, 0, 1) * lowerRayDistance);
    }
}
