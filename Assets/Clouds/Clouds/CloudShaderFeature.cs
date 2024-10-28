using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CloudShaderFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CloudShaderSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material material;
    }

    public CloudShaderSettings settings = new CloudShaderSettings();
    private CloudShaderPass cloudShaderPass;

    public override void Create()
    {
        cloudShaderPass = new CloudShaderPass(settings.renderPassEvent, settings.material);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
        {
            Debug.LogWarningFormat("Missing Material. {0} render pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
            return;
        }

        renderer.EnqueuePass(cloudShaderPass);
    }

    class CloudShaderPass : ScriptableRenderPass
    {
        private Material material;
        private RTHandle source;
        private RenderTargetHandle tempTexture;
        private static readonly Color clearColor = new Color(0, 0, 0, 0);

        public CloudShaderPass(RenderPassEvent renderPassEvent, Material material)
        {
            this.renderPassEvent = renderPassEvent;
            this.material = material;
            tempTexture.Init("_TempTexture");
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            renderingData.cameraData.requiresDepthTexture = true;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("CloudShaderPass");

            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            opaqueDesc.depthBufferBits = 0;
            opaqueDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R16G16B16A16_SFloat;

            // Get and clear temporary RT
            cmd.GetTemporaryRT(tempTexture.id, opaqueDesc);
            cmd.SetRenderTarget(tempTexture.Identifier());
            cmd.ClearRenderTarget(false, true, clearColor);

            // Perform blits
            Blit(cmd, source, tempTexture.Identifier(), material);
            Blit(cmd, tempTexture.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(tempTexture.id);
        }
    }
}








//     using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Rendering.Universal;

// public class CloudShaderFeature : ScriptableRendererFeature
// {
//     [System.Serializable]
//     public class CloudShaderSettings
//     {
//         public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
//         public Material material;
//     }

//     public CloudShaderSettings settings = new CloudShaderSettings();
//     private CloudShaderPass cloudShaderPass;

//     public override void Create()
//     {
//         cloudShaderPass = new CloudShaderPass(settings.renderPassEvent, settings.material);
//     }

//     public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
//     {
//         if (settings.material == null)
//         {
//             Debug.LogWarningFormat("Missing Material. {0} render pass will not execute. Check for missing reference in the assigned renderer.", GetType().Name);
//             return;
//         }

//         renderer.EnqueuePass(cloudShaderPass);
//     }

//     class CloudShaderPass : ScriptableRenderPass
//     {
//         private Material material;
//         private RTHandle source;
//         private RenderTargetHandle tempTexture;

//         public CloudShaderPass(RenderPassEvent renderPassEvent, Material material)
//         {
//             this.renderPassEvent = renderPassEvent;
//             this.material = material;
//             tempTexture.Init("_TempTexture");

//             // material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
//             // material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
//             // material.SetInt("_ZWrite", 0);
//             // material.EnableKeyword("_ALPHABLEND_ON");
//             // material.renderQueue = (int)RenderQueue.Transparent;

//         }

//         public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
//         {
//             source = renderingData.cameraData.renderer.cameraColorTargetHandle;
//             renderingData.cameraData.requiresDepthTexture = true;
//         }

//         public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
//         {
//             if (!Application.isPlaying) {
//                 return;
//             }

//             CommandBuffer cmd = CommandBufferPool.Get("CloudShaderPass");




//             RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
//             opaqueDesc.depthBufferBits = 0;
//             // opaqueDesc.graphicsFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm; // Ensure alpha is supported

//             cmd.GetTemporaryRT(tempTexture.id, opaqueDesc);
//             Blit(cmd, source, tempTexture.Identifier(), material);
//             Blit(cmd, tempTexture.Identifier(), source);

//             context.ExecuteCommandBuffer(cmd);
//             CommandBufferPool.Release(cmd);
//         }

//         public override void FrameCleanup(CommandBuffer cmd)
//         {
//             cmd.ReleaseTemporaryRT(tempTexture.id);
//         }
//     }
// }
