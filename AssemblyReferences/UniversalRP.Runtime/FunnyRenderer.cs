using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.SoFunny {
    public class FunnyRenderer : ScriptableRenderer {
        internal RenderTargetBufferSystem m_RenderTargetBufferSystem;
        DrawSkyboxPass m_DrawSkyboxPass;
        public FunnyRenderer(FunnyRendererData funnyRendererData) : base(funnyRendererData) {
            m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);
        }

        /// <summary>
        /// 设置Render所需要的内容
        /// </summary>
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData) {
            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;

            if (camera.clearFlags == CameraClearFlags.Skybox && cameraData.renderType != CameraRenderType.Overlay) {
                if (RenderSettings.skybox != null || (camera.TryGetComponent(out Skybox cameraSkybox) && cameraSkybox.material != null))
                    EnqueuePass(m_DrawSkyboxPass);
            }
        }
    }
}