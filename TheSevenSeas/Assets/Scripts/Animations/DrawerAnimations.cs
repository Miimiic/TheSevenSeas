using System.Collections.Generic;
using UnityEngine;

public class DrawerAnimator : MonoBehaviour
{
    [System.Serializable]
    public class Drawer
    {
        [Header("Drawer Settings")]
        public Transform drawerTransform;
        
        [Tooltip("The axis along which the drawer opens (relative to parent)")]
        public Axis openAxis = Axis.Z;
        
        [Tooltip("Minimum distance the drawer can open")]
        public float minOpenDistance = 0.2f;
        
        [Tooltip("Maximum distance the drawer can open")]
        public float maxOpenDistance = 0.5f;
        
        [Tooltip("Speed of the drawer animation")]
        public float animationSpeed = 2f;
        
        [Tooltip("Randomly determine open distance within min/max range")]
        public bool useRandomOpenDistance = true;
        
        [Header("Animation Curve (Optional)")]
        [Tooltip("Leave empty for linear movement, or assign a curve for custom easing")]
        public AnimationCurve openCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        // Runtime variables
        [HideInInspector] public Vector3 closedPosition;
        [HideInInspector] public Vector3 targetOpenPosition;
        [HideInInspector] public bool isOpen = false;
        [HideInInspector] public bool isAnimating = false;
        [HideInInspector] public float animationProgress = 0f;
        [HideInInspector] public float currentOpenDistance;
    }
    
    public enum Axis
    {
        X,
        Y,
        Z,
        NegativeX,
        NegativeY,
        NegativeZ
    }
    
    [Header("Drawers")]
    public List<Drawer> drawers = new List<Drawer>();
    
    //[Header("Interaction Settings")]
    public enum InteractionMode
    {
        ToggleAll,          // E key toggles all drawers
        LookAndPress,       // E key toggles drawer you're looking at
        ClickOnly,          // Only mouse click interaction
        ProximityBased      // E key toggles nearest drawer
    }
    
    [Tooltip("How the drawers should be interacted with")]
    public InteractionMode interactionMode = InteractionMode.LookAndPress;
    
    [Tooltip("Key to interact with drawers")]
    public KeyCode interactionKey = KeyCode.E;
    
    [Tooltip("Enable interaction by clicking on drawers")]
    public bool enableClickInteraction = true;
    
    [Tooltip("Maximum distance for raycast interaction")]
    public float interactionRange = 3f;
    
    [Tooltip("Layer mask for clickable drawers")]
    public LayerMask drawerLayerMask = -1;
    
    [Header("Debug")]
    [Tooltip("Show debug messages in console")]
    public bool showDebugMessages = false;
    
    private Camera mainCamera;
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Initialize all drawers
        foreach (Drawer drawer in drawers)
        {
            InitializeDrawer(drawer);
        }
    }
    
    private void InitializeDrawer(Drawer drawer)
    {
        if (drawer.drawerTransform == null)
        {
            Debug.LogWarning("Drawer transform is null!");
            return;
        }
        
        // Store the closed position (local position relative to parent)
        drawer.closedPosition = drawer.drawerTransform.localPosition;
        
        // Calculate random open distance
        if (drawer.useRandomOpenDistance)
        {
            drawer.currentOpenDistance = Random.Range(drawer.minOpenDistance, drawer.maxOpenDistance);
        }
        else
        {
            drawer.currentOpenDistance = drawer.maxOpenDistance;
        }
        
        // Calculate target open position based on axis
        Vector3 offset = GetAxisVector(drawer.openAxis) * drawer.currentOpenDistance;
        drawer.targetOpenPosition = drawer.closedPosition + offset;
        
        drawer.isOpen = false;
        drawer.isAnimating = false;
        drawer.animationProgress = 0f;
    }
    
    private void Update()
    {
        // Handle interaction key based on mode
        if (Input.GetKeyDown(interactionKey))
        {
            switch (interactionMode)
            {
                case InteractionMode.ToggleAll:
                    ToggleAllDrawers();
                    if (showDebugMessages) Debug.Log("Toggling all drawers");
                    break;
                    
                case InteractionMode.LookAndPress:
                    HandleLookInteraction();
                    break;
                    
                case InteractionMode.ProximityBased:
                    HandleProximityInteraction();
                    break;
                    
                case InteractionMode.ClickOnly:
                    // Do nothing on key press
                    break;
            }
        }
        
        // Click interaction
        if (enableClickInteraction && Input.GetMouseButtonDown(0))
        {
            HandleClickInteraction();
        }
        
        // Update all drawer animations
        foreach (Drawer drawer in drawers)
        {
            if (drawer.isAnimating)
            {
                AnimateDrawer(drawer);
            }
        }
    }
    
    private void HandleLookInteraction()
    {
        if (mainCamera == null)
        {
            if (showDebugMessages) Debug.LogWarning("Main camera not found!");
            return;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, drawerLayerMask))
        {
            // Check if the hit object is one of our drawers
            foreach (Drawer drawer in drawers)
            {
                if (drawer.drawerTransform != null && 
                    (hit.transform == drawer.drawerTransform || hit.transform.IsChildOf(drawer.drawerTransform)))
                {
                    ToggleDrawer(drawer);
                    if (showDebugMessages) Debug.Log($"Toggled drawer: {drawer.drawerTransform.name}");
                    return;
                }
            }
            
            if (showDebugMessages) Debug.Log($"Looking at {hit.transform.name} but it's not a registered drawer");
        }
        else
        {
            if (showDebugMessages) Debug.Log("Not looking at any collider within range");
        }
    }
    
    private void HandleProximityInteraction()
    {
        if (mainCamera == null) return;
        
        float nearestDistance = float.MaxValue;
        Drawer nearestDrawer = null;
        
        foreach (Drawer drawer in drawers)
        {
            if (drawer.drawerTransform == null) continue;
            
            float distance = Vector3.Distance(mainCamera.transform.position, drawer.drawerTransform.position);
            if (distance < nearestDistance && distance <= interactionRange)
            {
                nearestDistance = distance;
                nearestDrawer = drawer;
            }
        }
        
        if (nearestDrawer != null)
        {
            ToggleDrawer(nearestDrawer);
            if (showDebugMessages) Debug.Log($"Toggled nearest drawer: {nearestDrawer.drawerTransform.name}");
        }
        else
        {
            if (showDebugMessages) Debug.Log("No drawer within range");
        }
    }
    
    private void HandleClickInteraction()
    {
        if (mainCamera == null)
        {
            if (showDebugMessages) Debug.LogWarning("Main camera not found!");
            return;
        }
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, drawerLayerMask))
        {
            if (showDebugMessages) Debug.Log($"Clicked on: {hit.transform.name}");
            
            // Check if the hit object is one of our drawers
            foreach (Drawer drawer in drawers)
            {
                if (drawer.drawerTransform != null && 
                    (hit.transform == drawer.drawerTransform || hit.transform.IsChildOf(drawer.drawerTransform)))
                {
                    ToggleDrawer(drawer);
                    if (showDebugMessages) Debug.Log($"Toggled drawer: {drawer.drawerTransform.name}");
                    break;
                }
            }
        }
        else
        {
            if (showDebugMessages) Debug.Log("Click didn't hit anything in range or on correct layer");
        }
    }
    
    private void AnimateDrawer(Drawer drawer)
    {
        if (drawer.drawerTransform == null) return;
        
        // Update progress
        float progressDelta = Time.deltaTime * drawer.animationSpeed;
        
        if (drawer.isOpen)
        {
            drawer.animationProgress += progressDelta;
        }
        else
        {
            drawer.animationProgress -= progressDelta;
        }
        
        drawer.animationProgress = Mathf.Clamp01(drawer.animationProgress);
        
        // Apply animation curve if available
        float curveValue = drawer.openCurve != null ? 
            drawer.openCurve.Evaluate(drawer.animationProgress) : 
            drawer.animationProgress;
        
        // Lerp between closed and open positions
        drawer.drawerTransform.localPosition = Vector3.Lerp(
            drawer.closedPosition,
            drawer.targetOpenPosition,
            curveValue
        );
        
        // Stop animating when done
        if (drawer.animationProgress >= 1f || drawer.animationProgress <= 0f)
        {
            drawer.isAnimating = false;
        }
    }
    
    private Vector3 GetAxisVector(Axis axis)
    {
        switch (axis)
        {
            case Axis.X:
                return Vector3.right;
            case Axis.Y:
                return Vector3.up;
            case Axis.Z:
                return Vector3.forward;
            case Axis.NegativeX:
                return Vector3.left;
            case Axis.NegativeY:
                return Vector3.down;
            case Axis.NegativeZ:
                return Vector3.back;
            default:
                return Vector3.forward;
        }
    }
    
    // Public methods for external control
    
    public void ToggleDrawer(int drawerIndex)
    {
        if (drawerIndex >= 0 && drawerIndex < drawers.Count)
        {
            ToggleDrawer(drawers[drawerIndex]);
        }
    }
    
    public void ToggleDrawer(Drawer drawer)
    {
        if (drawer == null || drawer.drawerTransform == null) return;
        
        drawer.isOpen = !drawer.isOpen;
        drawer.isAnimating = true;
    }
    
    public void OpenDrawer(int drawerIndex)
    {
        if (drawerIndex >= 0 && drawerIndex < drawers.Count)
        {
            OpenDrawer(drawers[drawerIndex]);
        }
    }
    
    public void OpenDrawer(Drawer drawer)
    {
        if (drawer == null || drawer.drawerTransform == null) return;
        
        if (!drawer.isOpen)
        {
            drawer.isOpen = true;
            drawer.isAnimating = true;
        }
    }
    
    public void CloseDrawer(int drawerIndex)
    {
        if (drawerIndex >= 0 && drawerIndex < drawers.Count)
        {
            CloseDrawer(drawers[drawerIndex]);
        }
    }
    
    public void CloseDrawer(Drawer drawer)
    {
        if (drawer == null || drawer.drawerTransform == null) return;
        
        if (drawer.isOpen)
        {
            drawer.isOpen = false;
            drawer.isAnimating = true;
        }
    }
    
    public void ToggleAllDrawers()
    {
        foreach (Drawer drawer in drawers)
        {
            ToggleDrawer(drawer);
        }
    }
    
    public void OpenAllDrawers()
    {
        foreach (Drawer drawer in drawers)
        {
            OpenDrawer(drawer);
        }
    }
    
    public void CloseAllDrawers()
    {
        foreach (Drawer drawer in drawers)
        {
            CloseDrawer(drawer);
        }
    }
    
    public void RandomizeAllDrawerDistances()
    {
        foreach (Drawer drawer in drawers)
        {
            if (drawer.useRandomOpenDistance)
            {
                drawer.currentOpenDistance = Random.Range(drawer.minOpenDistance, drawer.maxOpenDistance);
                Vector3 offset = GetAxisVector(drawer.openAxis) * drawer.currentOpenDistance;
                drawer.targetOpenPosition = drawer.closedPosition + offset;
            }
        }
    }
    
    // Reset a specific drawer to closed position
    public void ResetDrawer(int drawerIndex)
    {
        if (drawerIndex >= 0 && drawerIndex < drawers.Count)
        {
            Drawer drawer = drawers[drawerIndex];
            if (drawer.drawerTransform != null)
            {
                drawer.drawerTransform.localPosition = drawer.closedPosition;
                drawer.isOpen = false;
                drawer.isAnimating = false;
                drawer.animationProgress = 0f;
            }
        }
    }
    
    public void ResetAllDrawers()
    {
        for (int i = 0; i < drawers.Count; i++)
        {
            ResetDrawer(i);
        }
    }
    
    // Gizmos to visualize drawer open positions in editor
    private void OnDrawGizmosSelected()
    {
        foreach (Drawer drawer in drawers)
        {
            if (drawer.drawerTransform == null) continue;
            
            // Draw the closed position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(drawer.drawerTransform.position, 0.05f);
            
            // Calculate and draw the open position
            Vector3 openOffset = GetAxisVector(drawer.openAxis) * 
                (drawer.useRandomOpenDistance ? drawer.maxOpenDistance : drawer.currentOpenDistance);
            Vector3 worldOpenPos = drawer.drawerTransform.parent != null ?
                drawer.drawerTransform.parent.TransformPoint(drawer.drawerTransform.localPosition + openOffset) :
                drawer.drawerTransform.position + openOffset;
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(worldOpenPos, 0.05f);
            
            // Draw line showing movement direction
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(drawer.drawerTransform.position, worldOpenPos);
        }
    }
}