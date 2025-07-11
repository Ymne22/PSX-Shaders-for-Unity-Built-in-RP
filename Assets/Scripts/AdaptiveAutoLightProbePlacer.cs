using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[ExecuteInEditMode]
public class AdaptiveAutoLightProbePlacer : MonoBehaviour
{
    [Header("Probe Placement Settings")]
    [Tooltip("Maximum distance between probes in dense areas")]
    public float minProbeSpacing = 1.0f;
    [Tooltip("Maximum distance between probes in empty areas")]
    public float maxProbeSpacing = 10.0f;
    [Tooltip("How quickly density falls off from objects")]
    public float densityFalloff = 2.0f;
    [Tooltip("Minimum height above surfaces for probes")]
    public float heightAboveSurface = 0.5f;

    [Header("Scene Analysis")]
    [Tooltip("Layer mask for objects that affect probe placement")]
    public LayerMask placementMask = -1;
    [Tooltip("Resolution of the initial analysis grid")]
    public float analysisGridSize = 5.0f;

    private LightProbeGroup probeGroup;

    [ContextMenu("Generate Adaptive Probes")]
    public void GenerateAdaptiveProbes()
    {
        InitializeProbeGroup();
        Bounds sceneBounds = CalculateSceneBounds();
        List<Vector3> probePositions = new List<Vector3>();

        // Create analysis grid
        Vector3 gridStart = sceneBounds.min;
        Vector3 gridEnd = sceneBounds.max;
        Vector3 gridSize = new Vector3(analysisGridSize, analysisGridSize, analysisGridSize);

        // Analyze scene and place probes
        for (float x = gridStart.x; x < gridEnd.x; x += analysisGridSize)
        {
            for (float z = gridStart.z; z < gridEnd.z; z += analysisGridSize)
            {
                // Find the highest surface in this column
                float surfaceHeight = FindSurfaceHeight(x, z, gridStart.y, gridEnd.y);

                if (surfaceHeight > -Mathf.Infinity)
                {
                    // Calculate local density based on nearby geometry
                    float densityFactor = CalculateLocalDensity(new Vector3(x, surfaceHeight, z));
                    float localSpacing = Mathf.Lerp(minProbeSpacing, maxProbeSpacing, 1 - densityFactor);

                    // Place probes in this area
                    PlaceProbesInArea(
                        new Vector3(x, surfaceHeight + heightAboveSurface, z),
                        localSpacing,
                        sceneBounds,
                        probePositions);
                }
            }
        }

        probeGroup.probePositions = probePositions.ToArray();
        Debug.Log($"Generated {probePositions.Count} adaptive light probes");
    }

    private void InitializeProbeGroup()
    {
        probeGroup = FindObjectOfType<LightProbeGroup>();
        if (probeGroup == null)
        {
            GameObject go = new GameObject("Adaptive Light Probe Group");
            probeGroup = go.AddComponent<LightProbeGroup>();
        }
    }

    private Bounds CalculateSceneBounds()
    {
        Bounds bounds = new Bounds();
        bool hasBounds = false;
        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer r in renderers)
        {
            if (!hasBounds)
            {
                bounds = r.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }

        if (!hasBounds) bounds = new Bounds(Vector3.zero, Vector3.one * 10f);
        return bounds;
    }

    private float FindSurfaceHeight(float x, float z, float minY, float maxY)
    {
        RaycastHit hit;
        Vector3 origin = new Vector3(x, maxY, z);
        if (Physics.Raycast(origin, Vector3.down, out hit, maxY - minY, placementMask))
        {
            return hit.point.y;
        }
        return -Mathf.Infinity;
    }

    private float CalculateLocalDensity(Vector3 position)
    {
        float maxDistance = maxProbeSpacing * 2f;
        Collider[] colliders = Physics.OverlapSphere(position, maxDistance, placementMask);

        if (colliders.Length == 0) return 0f;

        float totalInfluence = 0f;
        foreach (Collider col in colliders)
        {
            float distance = Vector3.Distance(position, col.ClosestPoint(position));
            float influence = 1 - Mathf.Clamp01(distance / maxDistance);
            influence = Mathf.Pow(influence, densityFalloff);
            totalInfluence += influence;
        }

        return Mathf.Clamp01(totalInfluence);
    }

    private void PlaceProbesInArea(Vector3 center, float spacing, Bounds bounds, List<Vector3> probes)
    {
        int gridSize = Mathf.CeilToInt(maxProbeSpacing / spacing);
        float startOffset = -gridSize * spacing * 0.5f;

        for (int x = 0; x <= gridSize; x++)
        {
            for (int z = 0; z <= gridSize; z++)
            {
                Vector3 probePos = center + new Vector3(
                    startOffset + x * spacing,
                    0,
                    startOffset + z * spacing);

                // Keep within bounds
                probePos.x = Mathf.Clamp(probePos.x, bounds.min.x, bounds.max.x);
                probePos.z = Mathf.Clamp(probePos.z, bounds.min.z, bounds.max.z);

                // Adjust height to surface
                RaycastHit hit;
                if (Physics.Raycast(probePos + Vector3.up * bounds.size.y, Vector3.down, out hit, bounds.size.y * 2, placementMask))
                {
                    probePos.y = hit.point.y + heightAboveSurface;
                }

                // Check if position is valid
                if (bounds.Contains(probePos) && !IsTooCloseToExisting(probePos, probes, spacing * 0.8f))
                {
                    probes.Add(probePos);
                }
            }
        }
    }

    private bool IsTooCloseToExisting(Vector3 pos, List<Vector3> existing, float minDistance)
    {
        foreach (Vector3 existingPos in existing)
        {
            if (Vector3.Distance(pos, existingPos) < minDistance)
                return true;
        }
        return false;
    }

    #if UNITY_EDITOR
    [MenuItem("Tools/Light Probes/Generate Adaptive Light Probe Volume")]
    private static void GenerateAdaptiveProbeVolume()
    {
        AdaptiveAutoLightProbePlacer generator = FindObjectOfType<AdaptiveAutoLightProbePlacer>();
        if (generator == null)
        {
            GameObject go = new GameObject("Generate Adaptive Light Probe Volume");
            generator = go.AddComponent<AdaptiveAutoLightProbePlacer>();
        }
        generator.GenerateAdaptiveProbes();
    }
    #endif
}