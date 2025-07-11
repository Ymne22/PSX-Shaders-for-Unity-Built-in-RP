using UnityEngine;

[ExecuteInEditMode]
public class PSXFogController : MonoBehaviour
{
    public PSXFogEffect targetFogEffect;
    
    [Header("Fog Settings")]
    [ColorUsage(false, true)]
    public Color fogColor = Color.gray;
    
    [Range(0.0f, 1000.0f)]
    public float fogStartDistance = 10.0f;
    
    [Range(0.0f, 1000.0f)]
    public float fogEndDistance = 50.0f;
    
    public bool coverSky = true;

    private void Awake()
    {
        if (targetFogEffect == null)
            targetFogEffect = Camera.main.GetComponent<PSXFogEffect>();
    }

    private void OnEnable()
    {
        UpdateFogEffect();
    }

    private void OnValidate()
    {
        UpdateFogEffect();
    }

    private void UpdateFogEffect()
    {
        if (targetFogEffect != null)
        {
            targetFogEffect.SetFogParameters(
                fogColor,
                fogStartDistance,
                fogEndDistance,
                coverSky
            );
        }
        else
        {
            Debug.LogWarning("PSXFogController: No target fog effect assigned!");
        }
    }
}