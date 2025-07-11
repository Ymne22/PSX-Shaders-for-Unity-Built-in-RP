using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
public class PSXFogEffect : MonoBehaviour
{
    public Shader fogShader;
    private Material _fogMaterial;
    [SerializeField] private Color _currentFogColor = Color.gray;
    [SerializeField] private float _currentFogStart = 10f;
    [SerializeField] private float _currentFogEnd = 50f;
    [SerializeField] private bool _currentCoverSky = true;
    private Camera _lastRenderingCamera;

    public void SetFogParameters(Color color, float start, float end, bool coverSky)
    {
        _currentFogColor = color;
        _currentFogStart = start;
        _currentFogEnd = end;
        _currentCoverSky = coverSky;
        UpdateMaterialProperties();
    }

    private void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
    }

    private void OnDisable()
    {
        if (_fogMaterial != null)
            DestroyImmediate(_fogMaterial);
    }

    private bool CreateMaterial()
    {
        if (fogShader == null || !fogShader.isSupported)
            return false;

        if (_fogMaterial == null || _fogMaterial.shader != fogShader)
        {
            _fogMaterial = new Material(fogShader);
            _fogMaterial.hideFlags = HideFlags.HideAndDontSave;
        }
        return true;
    }

    private void UpdateMaterialProperties()
    {
        if (!CreateMaterial()) return;
        
        _fogMaterial.SetColor("_FogColor", _currentFogColor);
        _fogMaterial.SetFloat("_FogStartDistance", _currentFogStart);
        _fogMaterial.SetFloat("_FogEndDistance", _currentFogEnd);
        _fogMaterial.SetInt("_CoverSky", _currentCoverSky ? 1 : 0);
        Shader.SetGlobalColor("_GlobalFogColor", _currentFogColor);
        Shader.SetGlobalFloat("_GlobalFogStart", _currentFogStart);
        Shader.SetGlobalFloat("_GlobalFogEnd", _currentFogEnd);
        Shader.SetGlobalInt("_GlobalCoverSky", _currentCoverSky ? 1 : 0);
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (!CreateMaterial())
        {
            Graphics.Blit(source, destination);
            return;
        }
        Camera renderingCamera = _lastRenderingCamera;
        if (Camera.current != null)
        {
            renderingCamera = Camera.current;
            _lastRenderingCamera = renderingCamera;
        }
        else if (renderingCamera == null)
        {
            renderingCamera = GetComponent<Camera>();
        }

        if (renderingCamera != null)
        {
            Matrix4x4 inverseProjection = renderingCamera.projectionMatrix.inverse;
            _fogMaterial.SetMatrix("_InverseProjection", inverseProjection);
            if (!Application.isPlaying)
            {
                UpdateMaterialProperties();
            }
        }

        Graphics.Blit(source, destination, _fogMaterial);
    }
    private void OnValidate()
    {
        UpdateMaterialProperties();
    }
}