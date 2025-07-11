using UnityEngine;

[ExecuteInEditMode]
[ImageEffectAllowedInSceneView]
[AddComponentMenu("Image Effects/PSX PostFX")]
public class PSX_PostFX : MonoBehaviour
{
    [Tooltip("The shader to apply the effect with.")]
    public Shader shader;

    [Tooltip("The number of colors per channel.")]
    [Range(2, 256)]
    public float colorDepth = 32;

    [Tooltip("The intensity of the dither pattern.")]
    [Range(0, 1)]
    public float ditherIntensity = 1.0f;

    [Tooltip("The effective screen resolution for the pixelation and dither pattern.")]
    public Vector2 pixelSize = new Vector2(320, 240);
    
    private Material material;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (shader == null)
        {
            Graphics.Blit(source, destination);
            return;
        }

        if (material == null)
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.HideAndDontSave;
        }

        material.SetFloat("_ColorDepth", colorDepth);
        material.SetFloat("_DitherIntensity", ditherIntensity);
        material.SetVector("_PixelSize", new Vector4(pixelSize.x, pixelSize.y, 0, 0));

        Graphics.Blit(source, destination, material);
    }
}