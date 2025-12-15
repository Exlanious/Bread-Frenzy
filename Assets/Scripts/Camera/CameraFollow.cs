using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform objectToFollow;
    public Vector3 offset = new Vector3(0, 2, -5);

    [Header("Follow Smoothness")]
    public float smoothTime = 0.25f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 100f;
    public float minYAngle = -35f;
    public float maxYAngle = 60f;

    [Header("Zoom Settings")]
    public float zoomSpeed = 2f;
    public float minZoomDistance = 2f;
    public float maxZoomDistance = 10f;

    [Header("Collision / Floor Settings")]
    public LayerMask collisionLayers;
    public float minDistanceAboveFloor = 0.5f;

    [Header("Scale / Size Settings")]
    public Transform scaleSource;         
    public float zoomPerScale = 1.0f;     
    public float heightPerScale = 1.0f;   

    private float baseMinZoom;
    private float baseMaxZoom;
    private float baseTargetHeight = 1.5f;
    private float baseMinDistanceAboveFloor;

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 currentVelocity;
    private float currentZoom;
    private int playerLayerMask;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Vector3 angles = transform.eulerAngles;
        rotationY = angles.y;
        rotationX = angles.x;

        currentZoom = offset.magnitude;

        playerLayerMask = 1 << objectToFollow.gameObject.layer;
        collisionLayers &= ~playerLayerMask;

        baseMinZoom = minZoomDistance;
        baseMaxZoom = maxZoomDistance;
        baseMinDistanceAboveFloor = minDistanceAboveFloor;
    }

    void LateUpdate()
    {
        if (objectToFollow == null) return;

        HandleRotation();
        HandleZoom();
        UpdateCameraPosition();

    }

    public void SetCameraYaw(float newYaw)
    {
        rotationY = newYaw;
    }

    void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, minYAngle, maxYAngle);
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoomDistance, maxZoomDistance);
        }
    }

    void UpdateCameraPosition()
    {
        if (scaleSource == null)
            scaleSource = objectToFollow;

        float scaleFactor = scaleSource != null ? scaleSource.localScale.x : 1f;
        scaleFactor = Mathf.Max(1f, scaleFactor);

        float dynamicMinZoom = baseMinZoom * scaleFactor * zoomPerScale;
        float dynamicMaxZoom = baseMaxZoom * scaleFactor * zoomPerScale;

        currentZoom = Mathf.Clamp(currentZoom, dynamicMinZoom, dynamicMaxZoom);

        Quaternion rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        Vector3 zoomedOffset = offset.normalized * currentZoom;
        Vector3 desiredPosition = objectToFollow.position + rotation * zoomedOffset;

        if (Physics.Raycast(desiredPosition, Vector3.down, out RaycastHit hit, Mathf.Infinity, collisionLayers))
        {
            float minY = objectToFollow.position.y + baseMinDistanceAboveFloor * scaleFactor;
            if (desiredPosition.y < minY)
                desiredPosition.y = minY;
        }

        Vector3 direction = desiredPosition - objectToFollow.position;
        if (Physics.Raycast(objectToFollow.position, direction.normalized, out hit, direction.magnitude, collisionLayers))
        {
            desiredPosition = hit.point;
        }

        transform.position = desiredPosition;

        float targetHeight = baseTargetHeight + (scaleFactor - 1f) * heightPerScale;
        transform.LookAt(objectToFollow.position + Vector3.up * targetHeight);
    }

}
