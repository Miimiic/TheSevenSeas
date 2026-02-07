using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController: MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float groundCheckDistance = 0.3f;
    public LayerMask groundMask;
    public GameObject flashlight; // Optional flashlight reference

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
    
    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool isBuildMode = false;
    
    // Mouse smoothing variables
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Initialize rotation from current camera angle
        if (cameraTransform != null)
        {
            xRotation = cameraTransform.localEulerAngles.x;
            if (xRotation > 180f)
                xRotation -= 360f;
        }
    }
    
    void Update()
    {
        // Toggle build mode
        if (Input.GetKeyDown(buildModeKey))
        {
            ToggleBuildMode();
        }
        
        // Toggle cursor lock with Escape (only in normal mode)
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
        
        // Always handle mouse look when cursor is locked (even in build mode)
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMouseLook();
        }
        
        CheckGround();
        
        // Allow jumping in both modes
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Flashlight toggle (optional, works in both modes)
        if (Input.GetKeyDown(KeyCode.F))
        {
            ToggleFlashlight();
        }
    }
    
    void FixedUpdate()
    {
        // Handle movement in both normal and build mode
        HandleMovement();
    }
    
    void HandleMouseLook()
    {
        // CRITICAL FIX: Use GetAxisRaw for frame-independent input
        // GetAxis has built-in smoothing that doesn't play well with Time.deltaTime
        Vector2 rawMouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );
        
        // Apply sensitivity
        Vector2 targetDelta = rawMouseDelta * mouseSensitivity;
        
        if (smoothMouse)
        {
            // Smooth the mouse movement using SmoothDamp (framerate-independent)
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
        
        // CRITICAL: Multiply by Time.deltaTime * 60 for framerate independence
        // This normalizes to 60 FPS baseline - same feel at any framerate
        float mouseX = currentMouseDelta.x * Time.deltaTime * 60f;
        float mouseY = currentMouseDelta.y * Time.deltaTime * 60f;
        
        // Vertical rotation (camera pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Horizontal rotation (character yaw)
        transform.Rotate(Vector3.up * mouseX);
    }
    
    void HandleMovement()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
        
        Vector3 moveDir = transform.right * x + transform.forward * z;
        Vector3 targetVelocity = moveDir.normalized * speed;
        
        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(velocity.x, 0f, velocity.z);
        
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }
    
    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
    
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

    void ToggleFlashlight()
    {
        if (flashlight != null)
        {
            flashlight.SetActive(!flashlight.activeSelf);
        }
    }
    
    void ToggleBuildMode()
    {
        isBuildMode = !isBuildMode;
        RefreshCursorState();
        
        Debug.Log(isBuildMode ? "Build Mode: ON" : "Build Mode: OFF");
    }
    
    public void RefreshCursorState()
    {
        // In build mode, keep cursor locked so camera can move
        // The grid system will handle mouse input for placement
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
    
    // Public methods for runtime adjustments
    public void SetSensitivity(float sensitivity)
    {
        mouseSensitivity = sensitivity;
    }
    
    public void SetSmoothing(float smoothAmount)
    {
        smoothing = Mathf.Clamp(smoothAmount, 1f, 20f);
    }
    
    // Public method for build mode status
    public bool IsBuildMode()
    {
        return isBuildMode;
    }
}