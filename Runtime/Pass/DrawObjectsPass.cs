using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

namespace UnityEngine.Rendering.SoFunny {
    /// <summary>
    /// 对场景物体进行渲染 透明物体和不透明物体都通过该Pass设置并渲染
    /// </summary>
    public class DrawObjectsPass : ScriptableRenderPass {
        PassData m_PassData;
        List<ShaderTagId> m_ShaderTagID = new List<ShaderTagId>();
        bool m_IsOpaque;
        ProfilingSampler m_ProfilingSampler;
        FilteringSettings m_FilteringSettings;
        RenderStateBlock m_RenderStateBlock;
        bool m_ShouldTransparentsReceiveShadows;
        public DrawObjectsPass(string profilerTag, bool opaque, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference) {
            base.profilingSampler = new ProfilingSampler(nameof(profilerTag));
            renderPassEvent = evt;
            m_PassData = new PassData();
            m_IsOpaque = opaque;
            // foreach(var shaderTagId in shaderTagIds){
            //     m_ShaderTagID.Add(shaderTagId);
            // }
            m_ProfilingSampler = new ProfilingSampler(profilerTag);
            m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
            m_ShouldTransparentsReceiveShadows = false;

            if (stencilState.enabled) {
                m_RenderStateBlock.stencilState = stencilState;
                m_RenderStateBlock.stencilReference = stencilReference;
                m_RenderStateBlock.mask = RenderStateMask.Stencil;
            }

            m_ShaderTagID.Add(new ShaderTagId("FRP"));
            m_ShaderTagID.Add(new ShaderTagId("SRPDefaultUnlit"));
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            m_PassData.m_FilteringSettings = m_FilteringSettings;
            m_PassData.m_IsOpaque = m_IsOpaque;
            m_PassData.m_ProfilingSampler = m_ProfilingSampler;
            m_PassData.m_RenderingData = renderingData;
            m_PassData.m_RenderStateBlock = m_RenderStateBlock;
            m_PassData.m_ShaderTagIdList = m_ShaderTagID;
            m_PassData.m_ShouldTransparentsReceiveShadows = m_ShouldTransparentsReceiveShadows;

            CameraSetup(m_PassData, ref renderingData);
            ExecutePass(context, m_PassData, ref renderingData);
        }

        /// <summary>
        /// 设置深度测试方式
        /// </summary>
        private static void CameraSetup(PassData data, ref RenderingData renderingData) {
            //if (renderingData.cameraData.renderer.useDepthPriming && data.m_IsOpaque && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth)) {
            // 桥接
            if (ScriptableRendererUtils.IsUseDepthPriming(renderingData.cameraData.renderer) && data.m_IsOpaque && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth)) {
                data.m_RenderStateBlock.depthState = new DepthState(false, CompareFunction.Equal);
                data.m_RenderStateBlock.mask |= RenderStateMask.Depth;
            } else if (data.m_RenderStateBlock.depthState.compareFunction == CompareFunction.Equal) {
                data.m_RenderStateBlock.depthState = new DepthState(true, CompareFunction.LessEqual);
                data.m_RenderStateBlock.mask |= RenderStateMask.Depth;
            }
        }

        /// <summary>
        /// 执行当前Pass的渲染
        /// </summary>
        private static void ExecutePass(ScriptableRenderContext context, PassData passData, ref RenderingData renderingData) {
            //CommandBuffer cmd = renderingData.commandBuffer;
            // 桥接
            CommandBuffer cmd = RenderingDataUtils.GetCommandBuffer(ref renderingData);
            using (new ProfilingScope(cmd, passData.m_ProfilingSampler)) {
                Camera camera = renderingData.cameraData.camera;

                // 渲染顺序的排列
                var sortFlags = (passData.m_IsOpaque) ? renderingData.cameraData.defaultOpaqueSortFlags : SortingCriteria.CommonTransparent;
                if (ScriptableRendererUtils.IsUseDepthPriming(renderingData.cameraData.renderer) && passData.m_IsOpaque && (renderingData.cameraData.renderType == CameraRenderType.Base || renderingData.cameraData.clearDepth))
                    sortFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
                FilteringSettings filterSettings = passData.m_FilteringSettings;

#if UNITY_EDITOR
                // When rendering the preview camera, we want the layer mask to be forced to Everything
                if (renderingData.cameraData.isPreviewCamera) {
                    filterSettings.layerMask = -1;
                }
#endif
                DrawingSettings drawSettings = RenderingUtils.CreateDrawingSettings(passData.m_ShaderTagIdList, ref renderingData, sortFlags);
                context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref filterSettings, ref passData.m_RenderStateBlock);

                /// 报错返回
                RenderingUtils.RenderObjectsWithError(context, ref renderingData.cullResults, camera, filterSettings, SortingCriteria.None);
            }
        }

        private class PassData {
            internal RenderingData m_RenderingData;

            internal bool m_IsOpaque;
            internal RenderStateBlock m_RenderStateBlock;
            internal FilteringSettings m_FilteringSettings;
            internal List<ShaderTagId> m_ShaderTagIdList;
            internal ProfilingSampler m_ProfilingSampler;

            internal bool m_ShouldTransparentsReceiveShadows;

            internal DrawObjectsPass pass;
        }
    }
}
