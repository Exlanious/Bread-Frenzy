using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;       // 主相机
    [SerializeField] private CharacterController controller;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Jump Settings")]
    [SerializeField] private float gravity = 9.81f;             // 基础重力
    [SerializeField] private float jumpHeight = 2f;             // 跳跃高度（保持不变）
    [SerializeField] private float jumpUpGravityMultiplier = 2.2f; // 上升阶段重力倍率（越大→上升越快）
    [SerializeField] private float fallGravityMultiplier = 3f;     // 下落阶段重力倍率

    [Header("Camera Settings")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float cameraDistance = 4f;
    [SerializeField] private float minPitch = -30f;
    [SerializeField] private float maxPitch = 60f;

    private float yaw;           // 水平旋转角
    private float pitch;         // 垂直旋转角
    private Vector3 velocity;    // 垂直速度
    private Transform camPivot;  // 相机旋转基点
    private bool isJumping;      // 是否处于跳跃阶段

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 自动创建相机 pivot
        camPivot = new GameObject("CameraPivot").transform;
        camPivot.position = transform.position + Vector3.up * 1.5f;
        playerCamera.SetParent(camPivot);
        playerCamera.localPosition = new Vector3(0, 0, -cameraDistance);
        playerCamera.localRotation = Quaternion.identity;
    }

    void Update()
    {
        HandleCamera();
        HandleMovement();
    }

    // -------------------- 摄像机控制 --------------------
    private void HandleCamera()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        camPivot.rotation = Quaternion.Euler(pitch, yaw, 0f);
        camPivot.position = transform.position + Vector3.up * 1.5f;
    }

    // -------------------- 玩家移动 + 跳跃 --------------------
    private void HandleMovement()
    {
        // 输入方向
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 inputDir = new Vector3(h, 0f, v).normalized;

        // 根据相机方向移动
        Vector3 camForward = camPivot.forward;
        Vector3 camRight = camPivot.right;
        camForward.y = 0;
        camRight.y = 0;
        Vector3 moveDir = (camForward * v + camRight * h).normalized;

        // ---- 跳跃逻辑 ----
        if (controller.isGrounded)
        {
            isJumping = false;
            velocity.y = -1f;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 计算固定高度的初始速度 √(2gh)
                velocity.y = Mathf.Sqrt(2f * gravity * jumpHeight);
                isJumping = true;
            }
        }
        else
        {
            // 上升阶段 → 加大重力使上升更快
            if (velocity.y > 0 && isJumping)
                velocity.y -= gravity * jumpUpGravityMultiplier * Time.deltaTime;
            // 下落阶段 → 更快下落
            else
                velocity.y -= gravity * fallGravityMultiplier * Time.deltaTime;
        }

        // ---- 实际移动 ----
        Vector3 finalMove = moveDir * moveSpeed + velocity;
        controller.Move(finalMove * Time.deltaTime);

        // ---- 转向 ----
        if (moveDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }
}
