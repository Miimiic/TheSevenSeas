using System.Collections;
using System.Linq.Expressions;
using UnityEngine;

public class MechMovementController : MonoBehaviour
{
    // This is a lot of coppied code from the player controller, However, that contains a lot of extra features that proved unnecessary for the Mech, so this is now used instead of the player controller
    // Also I made it a singleton, VIVE LA SINGLETONS !!! (P.s. This is mainly because the MechAbilityController is gonna have to fiddle with some of these numbers and I can NOT be bothered doing the extra lines I did in my Craft project cause that was inneficient as Fuh)

    [Header("Movement")]
    public float moveSpeed = 5f;
    [SerializeField] private bool hasDashed = false;

    // The mech needs movement damping to really *feel* B I G
    [Header("Movement Damping")]
    [SerializeField] private float currentMovementSmoothing;
    [SerializeField] private float baseDamping;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;

    [Tooltip("Smooth mouse movement")]
    public bool smoothMouse = true;

    [Tooltip("Smoothing amount (higher = smoother but more lag)")]
    [Range(1f, 20f)]
    public float smoothing = 5f;

    public Transform cameraTransform;

    [SerializeField] private Rigidbody rb;
    private float xRotation = 0f;

    // Mouse smoothing variables
    private Vector2 currentMouseDelta;
    private Vector2 currentMouseDeltaVelocity;

    public static MechMovementController Instance;


    void Start()
    {
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

        baseDamping = rb.linearDamping;



        // Incase the instance was not set to this, Set it to this script, if it is already set to something, KILL THIS OBJECT, IT SHOULD NOT BE ABLE TO EXIST
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Destroying gameObject due to multiple instances of the singleton MechMovementController class. Please make sure you havent spawned multiple Mechs.");
            Destroy(gameObject);
        }

    }

    void Update()
    {
        // Toggle cursor lock with Escape (only in normal mode)
        if (Input.GetKeyDown(KeyCode.Escape))
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

        // Always handle mouse look when cursor is locked
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            HandleMouseLook();
        }


    }

    void FixedUpdate()
    {


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

        Vector3 moveDir = transform.right * x + transform.forward * z;
        Vector3 targetVelocity = moveDir.normalized * moveSpeed;

        Vector3 velocity = rb.linearVelocity;
        Vector3 velocityChange = targetVelocity - new Vector3(velocity.x, 0f, velocity.z);

        if ((x != 0 || z != 0) && !hasDashed)
        {
            // Only do these if the player is currently controlling the movement, Otherwise use the dampening. I know this is Inneficient to do after all the vectory doohickeys, but I didnt make this and I am not asking Josh to explain this cause... I aint fucking understanding this
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
            rb.linearDamping = 6f;
            rb.angularDamping = 0.05f;

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


    // --- Dashing modifications ---
    
    // Only used by the Ability controller to allow the mech to slide a bit when it dashes for a second
    public void Dash()
    {
        Debug.Log("Altering Damping on mech");
        hasDashed = true;
        rb.linearDamping = 1;
        rb.angularDamping = 0;
        StartCoroutine(DashMovementFalloffCutoff());
    }

    // I know this name makes absolutely zero sense

    IEnumerator DashMovementFalloffCutoff()
    {
        currentMovementSmoothing = 0.1f;
        while (rb.linearDamping<=6f)
        {
            rb.linearDamping += currentMovementSmoothing*0.1f;
            currentMovementSmoothing += 0.1f;
            yield return new WaitForFixedUpdate();
        }

        rb.linearDamping = 6f;
        rb.angularDamping = 0.05f;
        hasDashed= false;
        yield break;
    }

}
