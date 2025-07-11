using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class FixedAutoLightProbePlacer : MonoBehaviour
{
    [Tooltip("Density of light probes (probes per unit)")]
    public float probeDensity = 0.5f;
    
    [Tooltip("Padding around scene bounds")]
    public float boundsPadding = 5f;
    
    [Tooltip("Reference to the Light Probe Group in the scene")]
    public LightProbeGroup lightProbeGroup;
    
    [ContextMenu("Generate Light Probes")]
    public void GenerateLightProbes()
    {
        if (lightProbeGroup == null)
        {
            lightProbeGroup = FindObjectOfType<LightProbeGroup>();
            if (lightProbeGroup == null)
            {
                GameObject go = new GameObject("Light Probe Group");
                lightProbeGroup = go.AddComponent<LightProbeGroup>();
            }
        }
        
        Bounds sceneBounds = GetSceneBounds();
        sceneBounds.Expand(boundsPadding);
        
        int xCount = Mathf.CeilToInt(sceneBounds.size.x * probeDensity);
        int yCount = Mathf.CeilToInt(sceneBounds.size.y * probeDensity);
        int zCount = Mathf.CeilToInt(sceneBounds.size.z * probeDensity);
        
        Vector3[] probePositions = new Vector3[xCount * yCount * zCount];
        
        int index = 0;
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                for (int z = 0; z < zCount; z++)
                {
                    float xPos = sceneBounds.min.x + (sceneBounds.size.x / (xCount - 1)) * x;
                    float yPos = sceneBounds.min.y + (sceneBounds.size.y / (yCount - 1)) * y;
                    float zPos = sceneBounds.min.z + (sceneBounds.size.z / (zCount - 1)) * z;
                    
                    probePositions[index] = new Vector3(xPos, yPos, zPos);
                    index++;
                }
            }
        }
        
        lightProbeGroup.probePositions = probePositions;
        
        Debug.Log($"Generated {probePositions.Length} light probes in a {xCount}x{yCount}x{zCount} grid.");
    }
    
    private Bounds GetSceneBounds()
    {
        Bounds bounds = new Bounds();
        bool hasBounds = false;
        
        Renderer[] renderers = FindObjectsOfType<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }
        

        if (!hasBounds)
        {
            bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        }
        
        return bounds;
    }
    
    #if UNITY_EDITOR
    [MenuItem("Tools/Light Probes/Generate Fixed Light Probe Volume")]
    private static void AutoGenerateMenu()
    {
        FixedAutoLightProbePlacer placer = FindObjectOfType<FixedAutoLightProbePlacer>();
        if (placer == null)
        {
            GameObject go = new GameObject("Generate Fixed Light Probe Volume");
            placer = go.AddComponent<FixedAutoLightProbePlacer>();
        }
        placer.GenerateLightProbes();
    }
    #endif
}