using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class MegaMapFogBypass : MonoBehaviour
{
    private Camera targetCamera;

    private bool cachedFogEnabled;
    private Color cachedFogColor;
    private float cachedFogDensity;

    private void OnEnable()
    {
        targetCamera = GetComponent<Camera>();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
        RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;

        
        RenderSettings.fog = cachedFogEnabled;
        RenderSettings.fogColor = cachedFogColor;
        RenderSettings.fogDensity = cachedFogDensity;
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam != targetCamera) return;

        cachedFogEnabled = RenderSettings.fog;
        cachedFogColor = RenderSettings.fogColor;
        cachedFogDensity = RenderSettings.fogDensity;

        RenderSettings.fog = false;
        RenderSettings.fogDensity = 0f;
    }

    private void OnEndCameraRendering(ScriptableRenderContext context, Camera cam)
    {
        if (cam != targetCamera) return;

        RenderSettings.fog = cachedFogEnabled;
        RenderSettings.fogColor = cachedFogColor;
        RenderSettings.fogDensity = cachedFogDensity;
    }
}
