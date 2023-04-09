using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.SoFunny {
    public class FunnyRenderer : ScriptableRenderer {
        // 桥接
        internal RenderTargetBufferSystemUtils m_ColorBufferSystem;
        DrawSkyboxPass m_DrawSkyboxPass;
        DrawObjectsPass m_DrawOpaquesPass;

        ForwardLights m_ForwardLights;

        StencilState m_DefaultStencilState;

#if UNITY_SWITCH || UNITY_ANDROID
        const GraphicsFormat k_DepthStencilFormat = GraphicsFormat.D24_UNorm_S8_UInt;
        const int k_DepthBufferBits = 24;
#else
        const GraphicsFormat k_DepthStencilFormat = GraphicsFormat.D32_SFloat_S8_UInt;
        const int k_DepthBufferBits = 32;
#endif

        public FunnyRenderer(FunnyRendererData funnyRendererData) : base(funnyRendererData) {
            // 模板测试设置
            StencilStateData stencilData = funnyRendererData.defaultStencilState;
            m_DefaultStencilState = StencilState.defaultValue;
            m_DefaultStencilState.enabled = stencilData.overrideStencilState;
            m_DefaultStencilState.SetCompareFunction(stencilData.stencilCompareFunction);
            m_DefaultStencilState.SetPassOperation(stencilData.passOperation);
            m_DefaultStencilState.SetFailOperation(stencilData.failOperation);
            m_DefaultStencilState.SetZFailOperation(stencilData.zFailOperation);

            m_DrawOpaquesPass = new DrawObjectsPass(FRPProfileId.DrawOpaqueObjects.GetType().Name, true, RenderPassEvent.BeforeRenderingOpaques, RenderQueueRange.opaque, funnyRendererData.opaqueLayerMask, m_DefaultStencilState, stencilData.stencilReference);
            m_DrawSkyboxPass = new DrawSkyboxPass(RenderPassEvent.BeforeRenderingSkybox);

            // 还不需要
            //m_ColorBufferSystem = new RenderTargetBufferSystemUtils("_CameraColorAttachment");

            m_ForwardLights = new ForwardLights();
        }

        /// <summary>
        /// 设置灯光参数
        /// </summary>
        public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData) {
            m_ForwardLights.Setup(context, ref renderingData);
        }

        /// <summary>
        /// 设置Render所需要的内容
        /// </summary>
        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData) {
            //m_ForwardLights.PreSetup(ref renderingData);

            ref CameraData cameraData = ref renderingData.cameraData;
            Camera camera = cameraData.camera;

            EnqueuePass(m_DrawOpaquesPass);
            if (camera.clearFlags == CameraClearFlags.Skybox && cameraData.renderType != CameraRenderType.Overlay) {
                if (RenderSettings.skybox != null || (camera.TryGetComponent(out Skybox cameraSkybox) && cameraSkybox.material != null))
                    EnqueuePass(m_DrawSkyboxPass);
            }
        }
    }
}