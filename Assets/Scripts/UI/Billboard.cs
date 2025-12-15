using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera cam;

    void LateUpdate()
    {
        // If there’s no cached camera, or if it got disabled, find one
        if (cam == null || !cam.isActiveAndEnabled)
            cam = Camera.main;

        if (cam == null) return;

        // Face the camera directly
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,
                         cam.transform.rotation * Vector3.up);
    }
}
