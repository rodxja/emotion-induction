using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullscreenEffectFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class Settings
    {
        public Material effectMaterial;
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingTransparents;
    }

    public Settings settings = new Settings();

    class FullscreenEffectPass : ScriptableRenderPass
    {
        Material material;

        public FullscreenEffectPass(Material mat)
        {
            material = mat;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (material == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("FullscreenEffect");

            RenderTargetIdentifier source = renderingData.cameraData.renderer.cameraColorTargetHandle;
            cmd.Blit(source, source, material);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    FullscreenEffectPass pass;

    public override void Create()
    {
        pass = new FullscreenEffectPass(settings.effectMaterial)
        {
            renderPassEvent = settings.passEvent
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}
