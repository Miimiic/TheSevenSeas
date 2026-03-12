using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask;
    public GameObject flashlight;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    [Tooltip("Smooth mouse movement")]
    public bool smoothMouse = true;
    [Tooltip("Smoothing amount (higher = smoother but more lag)")]
    [Range(1f, 20f)]
    public float smoothing = 5f;
    public Transform cameraTransform;

    [Header("Ground Check")]
    public Transform groundCheckPoint;
    public float groundCheckRadius = 0.3f;

    [Header("Build Mode")]
    public KeyCode buildModeKey = KeyCode.B;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;    // Per second while sprinting
    public float staminaRegenRate = 10f;    // Per second while not sprinting
    public float staminaRegenDelay = 2f;    // Seconds after sprinting before regen starts
    public float jumpStaminaCost = 15f;
    public RectMask2D staminaBarMask;

    private float currentStamina;
    private float timeSinceSprint;
    private bool isOutOfStamina = false;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool isBuildMode = false;

    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraTransform != null)
        {
            xRotation = cameraTransform.localEulerAngles.x;
            if (xRotation > 180f)
                xRotation -= 360f;
        }

        currentStamina = maxStamina;
        timeSinceSprint = staminaRegenDelay;
        HandleStaminaBar();
    }

    void Update()
    {
        if (Input.GetKeyDown(buildModeKey))
            ToggleBuildMode();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isBuildMode)
            {
                if (Cursor.lockState == CursorLockMode.Locked)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }

        if (Cursor.lockState == CursorLockMode.Locked)
            HandleMouseLook();

        CheckGround();

        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            // Only jump if enough stamina
            if (currentStamina >= jumpStaminaCost)
            {
                Jump();
                UseStamina(jumpStaminaCost);
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
            ToggleFlashlight();

        HandleStamina();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ------------------------------- Stamina -------------------------------

    void HandleStamina()
    {
        bool isSprinting = Input.GetKey(KeyCode.LeftShift) && IsMoving() && !isOutOfStamina;

        if (isSprinting)
        {
            timeSinceSprint = 0f;
            UseStamina(staminaDrainRate * Time.deltaTime);
        }
        else
        {
            timeSinceSprint += Time.deltaTime;

            // Only regen after delay and not already full
            if (timeSinceSprint >= staminaRegenDelay && currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);

                // Once stamina is full again, clear the out of stamina flag
                if (currentStamina >= maxStamina)
                    isOutOfStamina = false;

                HandleStaminaBar();
            }
        }
    }

    void UseStamina(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Max(currentStamina, 0f);

        if (currentStamina <= 0f)
            isOutOfStamina = true;

        HandleStaminaBar();
    }

    bool IsMoving()
    {
        return Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;
    }

    void HandleStaminaBar()
    {
        if (staminaBarMask == null) return;

        float staminaPercent = currentStamina / maxStamina;
        float rightPadding = (1f - staminaPercent) * 700f;

        staminaBarMask.padding = new Vector4(
            staminaBarMask.padding.x, // left
            staminaBarMask.padding.y, // bottom
            rightPadding,             // right
            staminaBarMask.padding.w  // top
        );
    }

    // ------------------------------- Mouse Look -------------------------------

    void HandleMouseLook()
    {
        Vector2 rawMouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );

        Vector2 targetDelta = rawMouseDelta * mouseSensitivity;

        if (smoothMouse)
        {
            currentMouseDelta = Vector2.SmoothDamp(
                currentMouseDelta,
                targetDelta,
                ref currentMouseDeltaVelocity,
                1f / smoothing,
                Mathf.Infinity,
                Time.deltaTime
            );
        }
        else
        {
            currentMouseDelta = targetDelta;
        }

        float mouseX = currentMouseDelta.x * Time.deltaTime * 60f;
        float mouseY = currentMouseDelta.y * Time.deltaTime * 60f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ------------------------------- Movement -------------------------------

    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        // Can only sprint if not out of stamina
        bool canSprint = Input.GetKey(KeyCode.LeftShift) && !isOutOfStamina;
        float speed = canSprint ? sprintSpeed : moveSpeed;

        Vector3 moveDir = transform.right * x + transform.forward * z;
        Vector3 targetVelocity = moveDir.normalized * speed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(velocity.x, 0f, velocity.z);

        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    void Jump()
    {
        // Zero out vertical velocity first so jump height is always consistent
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // ------------------------------- Ground Check -------------------------------

    void CheckGround()
    {
        if (groundCheckPoint == null)
        {
            Debug.LogWarning("GroundCheckPoint not assigned!");
            return;
        }

        isGrounded = Physics.CheckSphere(
            groundCheckPoint.position,
            groundCheckRadius,
            groundMask
        );
    }

    // ------------------------------- Misc -------------------------------

    void ToggleFlashlight()
    {
        if (flashlight != null)
            flashlight.SetActive(!flashlight.activeSelf);
    }

    void ToggleBuildMode()
    {
        isBuildMode = !isBuildMode;
        RefreshCursorState();
        Debug.Log(isBuildMode ? "Build Mode: ON" : "Build Mode: OFF");
    }

    public void RefreshCursorState()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }

    public void SetSensitivity(float sensitivity) => mouseSensitivity = sensitivity;
    public void SetSmoothing(float smoothAmount) => smoothing = Mathf.Clamp(smoothAmount, 1f, 20f);
    public bool IsBuildMode() => isBuildMode;
    public float GetStamina() => currentStamina;
    public float GetMaxStamina() => maxStamina;
}