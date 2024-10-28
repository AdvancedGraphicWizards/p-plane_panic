using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudManager : MonoBehaviour
{
 private ScriptableRendererFeature customRenderFeature;

    void Start()
    {
        // Find and enable render pass feature
        var urpAsset = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
        var rendererDataField = typeof(UniversalRenderPipelineAsset).GetField("m_RendererDataList", BindingFlags.NonPublic | BindingFlags.Instance);
        var rendererDataList = rendererDataField.GetValue(urpAsset) as ScriptableRendererData[];
        var rendererData = rendererDataList[0];
        foreach (var feature in rendererData.rendererFeatures) {
            if (feature is CloudShaderFeature) {
                customRenderFeature = feature;
                break;
            }
        }
        Toggle(true);
    }

    public void OnDestroy() {
        Toggle(false);

    }

    public void Toggle(bool enabled)
    {
        if (customRenderFeature != null) {
            customRenderFeature.SetActive(enabled);
        }
    }

}
