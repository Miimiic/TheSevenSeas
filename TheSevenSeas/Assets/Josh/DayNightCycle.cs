using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    [Tooltip("Time acceleration: 1 game second = X real seconds")]
    public float timeScale = 30f;
    
    [Tooltip("Starting hour (0-24)")]
    [Range(0f, 24f)]
    public float startHour = 6f;
    
    [Header("Sun/Moon")]
    [Tooltip("Directional light acting as sun/moon")]
    public Light directionalLight;
    
    [Tooltip("Rotation axis for sun movement (X rotation simulates day cycle)")]
    public Vector3 rotationAxis = new Vector3(1f, 0f, 0f);
    
    [Header("Fog Settings")]
    [Tooltip("Enable fog density changes")]
    public bool manageFogDensity = true;
    
    [Tooltip("Fog density at midnight/night")]
    public float nightFogDensity = 0.05f;
    
    [Tooltip("Fog density at sunrise/sunset")]
    public float sunriseSunsetFogDensity = 0.03f;
    
    [Tooltip("Fog density at midday (lowest point)")]
    public float middayFogDensity = 0.01f;
    
    [Tooltip("Hour when fog reaches minimum (midday)")]
    [Range(0f, 24f)]
    public float middayHour = 12f;
    
    [Tooltip("Use animation curve for more control over fog changes")]
    public bool useCustomFogCurve = false;
    
    [Tooltip("Custom fog curve (0-1 on X axis = 0-24 hours, Y axis = fog multiplier)")]
    public AnimationCurve fogCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
    
    [Header("Light Intensity")]
    [Tooltip("Adjust sun intensity based on time")]
    public bool manageLightIntensity = true;
    
    [Tooltip("Light intensity at night")]
    public float nightIntensity = 0.1f;
    
    [Tooltip("Light intensity at day")]
    public float dayIntensity = 1.0f;
    
    [Header("Ambient Light")]
    [Tooltip("Change ambient light color based on time")]
    public bool manageAmbientLight = true;
    
    [Tooltip("Ambient color at night")]
    public Color nightAmbientColor = new Color(0.2f, 0.2f, 0.3f);
    
    [Tooltip("Ambient color at sunrise")]
    public Color sunriseAmbientColor = new Color(1f, 0.6f, 0.4f);
    
    [Tooltip("Ambient color at day")]
    public Color dayAmbientColor = new Color(0.8f, 0.8f, 0.8f);
    
    [Tooltip("Ambient color at sunset")]
    public Color sunsetAmbientColor = new Color(1f, 0.5f, 0.3f);
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    // Private variables
    private float currentTimeOfDay; // 0-24 hours
    private float sunAngle;
    
    void Start()
    {
        // Initialize time
        currentTimeOfDay = startHour;
        
        // Find directional light if not assigned
        if (directionalLight == null)
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    directionalLight = light;
                    break;
                }
            }
            
            if (directionalLight == null)
            {
                Debug.LogWarning("No directional light found! Please assign one.");
            }
        }
        
        // Enable fog if managing it
        if (manageFogDensity)
        {
            RenderSettings.fog = true;
        }
        
        // Initial update
        UpdateDayNightCycle();
    }
    
    void Update()
    {
        // Advance time (1 game second = timeScale real seconds)
        // 24 hours / 86400 seconds = 0.000277777... per second
        // Multiply by timeScale to accelerate
        currentTimeOfDay += (Time.deltaTime / 3600f) * timeScale;
        
        // Wrap around at 24 hours
        if (currentTimeOfDay >= 24f)
        {
            currentTimeOfDay -= 24f;
        }
        
        UpdateDayNightCycle();
    }
    
    void UpdateDayNightCycle()
    {
        // Calculate sun angle (0 hours = 0°, 12 hours = 180°, 24 hours = 360°)
        sunAngle = (currentTimeOfDay / 24f) * 360f;
        
        // Update sun rotation
        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(rotationAxis * sunAngle);
        }
        
        // Update fog
        if (manageFogDensity)
        {
            UpdateFogDensity();
        }
        
        // Update light intensity
        if (manageLightIntensity && directionalLight != null)
        {
            UpdateLightIntensity();
        }
        
        // Update ambient light
        if (manageAmbientLight)
        {
            UpdateAmbientLight();
        }
    }
    
    void UpdateFogDensity()
    {
        float targetDensity;
        
        if (useCustomFogCurve)
        {
            // Use custom curve
            float normalizedTime = currentTimeOfDay / 24f;
            float curveValue = fogCurve.Evaluate(normalizedTime);
            targetDensity = Mathf.Lerp(middayFogDensity, nightFogDensity, curveValue);
        }
        else
        {
            // Calculate fog based on time of day
            // Fog is highest at midnight (0/24), lowest at midday (12)
            float timeDifference = Mathf.Abs(currentTimeOfDay - middayHour);
            
            // If we're past midday, calculate from the other direction
            if (timeDifference > 12f)
            {
                timeDifference = 24f - timeDifference;
            }
            
            // Normalize to 0-1 (0 = midday, 1 = midnight)
            float normalizedTime = timeDifference / 12f;
            
            // Create smooth curve using sine wave
            float fogFactor = Mathf.Sin(normalizedTime * Mathf.PI * 0.5f);
            
            // Interpolate between midday and night fog
            targetDensity = Mathf.Lerp(middayFogDensity, nightFogDensity, fogFactor);
        }
        
        // Apply fog density
        RenderSettings.fogDensity = targetDensity;
    }
    
    void UpdateLightIntensity()
    {
        // Calculate intensity based on sun height
        // Sun is "up" roughly between 6-18 hours
        float intensity;
        
        if (currentTimeOfDay >= 6f && currentTimeOfDay <= 18f)
        {
            // Daytime - calculate how high the sun is
            float dayProgress = (currentTimeOfDay - 6f) / 12f; // 0-1
            float sineCurve = Mathf.Sin(dayProgress * Mathf.PI);
            intensity = Mathf.Lerp(nightIntensity, dayIntensity, sineCurve);
        }
        else
        {
            // Nighttime
            intensity = nightIntensity;
        }
        
        directionalLight.intensity = intensity;
    }
    
    void UpdateAmbientLight()
    {
        Color targetColor;
        
        // Dawn: 5-7
        if (currentTimeOfDay >= 5f && currentTimeOfDay < 7f)
        {
            float t = (currentTimeOfDay - 5f) / 2f;
            targetColor = Color.Lerp(nightAmbientColor, sunriseAmbientColor, t);
        }
        // Morning to Midday: 7-12
        else if (currentTimeOfDay >= 7f && currentTimeOfDay < 12f)
        {
            float t = (currentTimeOfDay - 7f) / 5f;
            targetColor = Color.Lerp(sunriseAmbientColor, dayAmbientColor, t);
        }
        // Midday to Evening: 12-17
        else if (currentTimeOfDay >= 12f && currentTimeOfDay < 17f)
        {
            targetColor = dayAmbientColor;
        }
        // Sunset: 17-19
        else if (currentTimeOfDay >= 17f && currentTimeOfDay < 19f)
        {
            float t = (currentTimeOfDay - 17f) / 2f;
            targetColor = Color.Lerp(dayAmbientColor, sunsetAmbientColor, t);
        }
        // Dusk: 19-21
        else if (currentTimeOfDay >= 19f && currentTimeOfDay < 21f)
        {
            float t = (currentTimeOfDay - 19f) / 2f;
            targetColor = Color.Lerp(sunsetAmbientColor, nightAmbientColor, t);
        }
        // Night: 21-5
        else
        {
            targetColor = nightAmbientColor;
        }
        
        RenderSettings.ambientLight = targetColor;
    }
    
    // Public methods to control time
    public void SetTimeOfDay(float hour)
    {
        currentTimeOfDay = Mathf.Clamp(hour, 0f, 24f);
        UpdateDayNightCycle();
    }
    
    public float GetTimeOfDay()
    {
        return currentTimeOfDay;
    }
    
    public string GetTimeString()
    {
        int hours = Mathf.FloorToInt(currentTimeOfDay);
        int minutes = Mathf.FloorToInt((currentTimeOfDay - hours) * 60f);
        return $"{hours:00}:{minutes:00}";
    }
    
    public void PauseTime()
    {
        enabled = false;
    }
    
    public void ResumeTime()
    {
        enabled = true;
    }
    
    void OnGUI()
    {
        if (showDebugInfo)
        {
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.UpperLeft;
            
            GUI.Label(new Rect(10, 10, 300, 30), $"Time: {GetTimeString()}", style);
            GUI.Label(new Rect(10, 35, 300, 30), $"Fog Density: {RenderSettings.fogDensity:F4}", style);
            
            if (directionalLight != null)
            {
                GUI.Label(new Rect(10, 60, 300, 30), $"Light Intensity: {directionalLight.intensity:F2}", style);
            }
        }
    }
    
    void OnValidate()
    {
        // Update in editor when values change
        if (Application.isPlaying)
        {
            UpdateDayNightCycle();
        }
    }
}