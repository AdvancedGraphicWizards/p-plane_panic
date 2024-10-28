using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudManager : MonoBehaviour
{
    [SerializeField] CloudParams cloudParams;

    [SerializeField] private Transform cloudBoundingBox;
    [SerializeField] private Camera cam;
    private ScriptableRendererFeature customRenderFeature;

    void Update() {
        float cameraPos = cam.transform.position.z;
        float cloudPos = cloudBoundingBox.position.z + cloudBoundingBox.localScale.z / 2;

        if (cameraPos > cloudPos) {
            float dist = Random.Range(cloudParams.minDistance, cloudParams.maxDistance);
            Debug.Log($"DIST {dist}");
            cloudBoundingBox.position += new Vector3(0,0, dist);
        }
    }
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
