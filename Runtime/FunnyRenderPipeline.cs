using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;

namespace UnityEngine.Rendering.SoFunny {
    internal enum FRPProfileId {
        // CPU
        FunnyRenderTotal,
        RenderCameraStack,
        DrawOpaqueObjects
    }
    public class FunnyRenderPipeline : RenderPipeline {
        internal const int defaultRenderingLayerMask = 0x00000001;

        private readonly FunnyRenderPipelineAsset pipelineAsset;

        public static FunnyRenderPipelineAsset asset {
            get => GraphicsSettings.currentRenderPipeline as FunnyRenderPipelineAsset;
        }

        public FunnyRenderPipeline(FunnyRenderPipelineAsset asset) {
            this.pipelineAsset = asset;
            RTHandles.Initialize(Screen.width, Screen.height);
        }

        /// Camera Render Start
#if UNITY_2021_1_OR_NEWER
        /// <inheritdoc/>
        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
            Render(context, new List<Camera>(cameras));
        }
#endif

#if UNITY_2021_1_OR_NEWER
        /// <inheritdoc/>
        protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
#else
        /// <inheritdoc/>
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
#endif
        {
            using var profScope = new ProfilingScope(null, ProfilingSampler.Get(FRPProfileId.FunnyRenderTotal));
#if UNITY_2021_1_OR_NEWER
            using (new ProfilingScope(null, Profiling.Pipeline.beginContextRendering)) {
                BeginContextRendering(context, cameras);
            }
#else
            using (new ProfilingScope(null, Profiling.Pipeline.beginFrameRendering)){
                BeginFrameRendering(context, cameras);
            }
#endif
            GraphicsSettings.lightsUseLinearIntensity = (QualitySettings.activeColorSpace == ColorSpace.Linear);
            GraphicsSettings.lightsUseColorTemperature = true;
            GraphicsSettings.defaultRenderingLayerMask = defaultRenderingLayerMask;

            //UniversalRenderPipeline.SortCameras(cameras);
            SortCameras(cameras);
#if UNITY_2021_1_OR_NEWER
            for (int i = 0; i < cameras.Count; ++i)
#else
            for(int i = 0; i < cameras.Length; ++i)
#endif
            {
                var camera = cameras[i];
                camera.allowDynamicResolution = false;
                if (UniversalRenderPipeline.IsGameCamera(camera)) {
                    RenderCameraStack(context, camera);
                } else {
                    using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering)) {
                        BeginCameraRendering(context, camera);
                    }
                    // Volume控件 暂时不需要
                    //UpdateVolumeFramework(camera, null);

                    RenderSingleCameraInternal(context, camera);
                    using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering)) {
                        EndCameraRendering(context, camera);
                    }
                }
            }

#if UNITY_2021_1_OR_NEWER
            using (new ProfilingScope(null, Profiling.Pipeline.endContextRendering)) {
                EndContextRendering(context, cameras);
            }
#else
            using (new ProfilingScope(null, Profiling.Pipeline.endFrameRendering))
            {
                EndFrameRendering(context, cameras);
            }
#endif
        }


        /// <summary>
        /// 渲染一个摄像机堆栈，将最后一个摄像机的结果渲染到屏幕
        /// </summary>
        static void RenderCameraStack(ScriptableRenderContext context, Camera camera) {
            using var profScope = new ProfilingScope(null, ProfilingSampler.Get(FRPProfileId.RenderCameraStack));

            camera.TryGetComponent<FunnyAdditionalCameraData>(out var baseCameraAdditionalData);
            // Overlay cameras will be rendered stacked while rendering base cameras
            if (baseCameraAdditionalData != null && baseCameraAdditionalData.renderType == CameraRenderType.Overlay)
                return;

            bool anyPostProcessingEnabled = baseCameraAdditionalData != null && baseCameraAdditionalData.renderPostProcessing;

            /// UI相机使用同一个相机渲染 不使用相机堆栈
            int lastActiveOverlayCameraIndex = -1;
            bool isStackedRendering = lastActiveOverlayCameraIndex != -1;
            using (new ProfilingScope(null, Profiling.Pipeline.beginCameraRendering)) {
                BeginCameraRendering(context, camera);
            }

            /// 后处理Volume组件
            //UpdateVolumeFramework(camera, baseCameraAdditionalData);
            InitializeCameraData(camera, baseCameraAdditionalData, !isStackedRendering, out var baseCameraData);
            //RenderTextureDescriptor originalTargetDesc = baseCameraData.cameraTargetDescriptor;
            RenderSingleCamera(context, ref baseCameraData, anyPostProcessingEnabled);

            using (new ProfilingScope(null, Profiling.Pipeline.endCameraRendering)) {
                EndCameraRendering(context, camera);
            }

        }

        internal static void RenderSingleCameraInternal(ScriptableRenderContext context, Camera camera) {
            FunnyAdditionalCameraData additionalCameraData = null;
            /////////////////////////////////////
            // 只是做一个代码保护
            if (UniversalRenderPipeline.IsGameCamera(camera)) {
                camera.gameObject.TryGetComponent(out additionalCameraData);
            }

            if (additionalCameraData != null && additionalCameraData.renderType != CameraRenderType.Base) {
                Debug.LogWarning("Only Base cameras can be rendered with standalone RenderSingleCamera. Camera will be skipped.");
                return;
            }

            /////////////////////////////////////
            InitializeCameraData(camera, additionalCameraData, true, out var cameraData);
            RenderSingleCamera(context, ref cameraData, cameraData.postProcessEnabled);
        }

        /// <summary>
        /// 单个摄像机渲染
        /// </summary>
        static void RenderSingleCamera(ScriptableRenderContext context, ref CameraData cameraData, bool anyPostProcessingEnabled) {
            Camera camera = cameraData.camera;
            var render = cameraData.renderer;
            if (render == null) {
                Debug.LogWarning(string.Format("Can't find renderer for camera, quit rendering", camera.name));
                return;
            }

            if (!TryGetCullingParameters(cameraData, out var cullingParameters)) {
                return;
            }

            //ScriptableRenderer.current = render;
            // 桥接
            ScriptableRendererUtils.SetRenderer(render);
            CommandBuffer cmd = CommandBufferPool.Get();
            // xr is not needed
            CommandBuffer cmdScope = cmd;
            ProfilingSampler sampler = Profiling.TryGetOrAddCameraSampler(camera);

            using (new ProfilingScope(cmdScope, sampler)) {
                //render.Clear(cameraData.renderType);
                // 桥接
                ScriptableRendererUtils.Clear(render, cameraData.renderType);
                using (new ProfilingScope(null, Profiling.Pipeline.Renderer.setupCullingParameters)) {
                    /// RenderPass 的剔除 FunnyRender需要重新定义这两个函数 现在没有
                    //render.OnPreCullRenderPasses(in cameraData);
                    // 桥接
                    ScriptableRendererUtils.OnPreCullRenderPasses(render, in cameraData);
                    render.SetupCullingParameters(ref cullingParameters, ref cameraData);
                }

                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
#if UNITY_EDITOR
                // Emit scene view UI
                if (cameraData.isSceneViewCamera)
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
                else
#endif
                /// 提交UI mesh ？
                if (cameraData.camera.targetTexture != null && cameraData.cameraType != CameraType.Preview)
                    ScriptableRenderContext.EmitGeometryForCamera(camera);

                var cullResults = context.Cull(ref cullingParameters);
                InitializeRenderingData(asset, ref cameraData, ref cullResults, anyPostProcessingEnabled, cmd, out var renderingData);

                /// 设置 RTHandles 最大尺寸为摄像机尺寸
                /// RTHandles 官方介绍
                /// https://docs.unity3d.com/Packages/com.unity.render-pipelines.core@12.0/manual/rthandle-system-fundamentals.html
                RTHandles.SetReferenceSize(cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height);

                /// 插入 Render Pass
                //render.AddRenderPasses(ref renderingData);
                // 桥接
                ScriptableRendererUtils.AddRenderPasses(render, ref renderingData);

                using (new ProfilingScope(null, Profiling.Pipeline.Renderer.setup)) {
                    render.Setup(context, ref renderingData);
                }

                render.Execute(context, ref renderingData);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
            using (new ProfilingScope(null, Profiling.Pipeline.Context.submit)) {
                /// 实际执行context的命令
                context.Submit();
            }

            //ScriptableRenderer.current = null;
            // 桥接
            ScriptableRendererUtils.SetRendererNull();

        }

        /// <summary>
        /// 获取 FunnyAdditionalCameraData 信息
        /// </summary>
        static void InitializeCameraData(Camera camera, FunnyAdditionalCameraData additionalCameraData, bool resolveFinalTarget, out CameraData cameraData) {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeCameraData);

            cameraData = new CameraData();

            InitializeStackedCameraData(camera, additionalCameraData, ref cameraData);
            InitializeAdditionalCameraData(camera, additionalCameraData, resolveFinalTarget, ref cameraData);

            /// var renderer = additionalCameraData != null ? additionalCameraData.scriptableRenderer : null;
            var renderer = additionalCameraData?.scriptableRenderer;
            bool rendererSupportsMSAA = renderer != null && renderer.supportedRenderingFeatures.msaa;

            bool isSceneOrPreviewCamera = camera.cameraType == CameraType.SceneView || cameraData.cameraType == CameraType.Preview;
            float renderScale = isSceneOrPreviewCamera ? 1.0f : cameraData.renderScale;

            // cameraData.hdrColorBufferPrecision = asset ? asset.hdrColorBufferPrecision : HDRColorBufferPrecision._32Bits;
            // cameraData.cameraTargetDescriptor = UniversalRenderPipeline.CreateRenderTextureDescriptor(camera, renderScale,
            // cameraData.isHdrEnabled, cameraData.hdrColorBufferPrecision, msaaSamples, needsAlphaChannel, cameraData.requiresOpaqueTexture);
            cameraData.cameraTargetDescriptor = CreateRenderTextureDescriptor(camera);
        }

        /// <summary>
        /// 设置相机中通用的设置 所有设置都是主相机的参数
        /// </summary>
        static void InitializeStackedCameraData(Camera camera, FunnyAdditionalCameraData baseAdditionalCameraData, ref CameraData cameraData) {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeStackedCameraData);

            var setting = asset;
            cameraData.targetTexture = camera.targetTexture;
            cameraData.cameraType = camera.cameraType;

            //cameraData.isHdrEnabled = camera.allowHDR && setting.supportsHDR;

            Rect cameraRect = camera.rect;
            /// internal 限制
            //cameraData.pixelRect = camera.pixelRect;
            //cameraData.pixelWidth = camera.pixelWidth;
            //cameraData.pixelHeight = camera.pixelHeight;
            //cameraData.aspectRatio = (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
            // 桥接
            CameraDataUtils.SetPixelRect(ref cameraData, camera.pixelRect);
            CameraDataUtils.SetPixelWidth(ref cameraData, camera.pixelWidth);
            CameraDataUtils.SetPixelHeight(ref cameraData, camera.pixelHeight);
            CameraDataUtils.SetAspectRatio(ref cameraData);

            cameraData.isDefaultViewport = (!(Math.Abs(cameraRect.x) > 0.0f || Math.Abs(cameraRect.y) > 0.0f ||
            Math.Abs(cameraRect.width) < 1.0f || Math.Abs(cameraRect.height) < 1.0f));

            //cameraData.xr = XRSystem.emptyPass;
            // 桥接
            CameraDataUtils.DisableXR(ref cameraData);

            var commonOpaqueFlags = SortingCriteria.CommonOpaque;
            var noFrontToBackOpaqueFlags = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
            bool hasHSRGPU = SystemInfo.hasHiddenSurfaceRemovalOnGPU;
            bool canSkipFrontToBackSorting = (camera.opaqueSortMode == OpaqueSortMode.Default && hasHSRGPU) || camera.opaqueSortMode == OpaqueSortMode.NoDistanceSort;
            /// 如果硬件支持HSRAO（深度预通滤镜）或者 摄像机设置不按距离排序 则默认排序方式不适用前到后的排列顺序
            cameraData.defaultOpaqueSortFlags = canSkipFrontToBackSorting ? noFrontToBackOpaqueFlags : commonOpaqueFlags;
        }

        /// <summary>
        /// 设置堆栈相机可以不同的设置
        /// </summary>
        static void InitializeAdditionalCameraData(Camera camera, FunnyAdditionalCameraData additionalCameraData, bool resolveFinalTarget, ref CameraData cameraData) {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeAdditionalCameraData);

            var setting = asset;

            cameraData.camera = camera;
            bool isSceneViewCamera = cameraData.isSceneViewCamera;

            if (isSceneViewCamera) {
                cameraData.renderType = CameraRenderType.Base;
                cameraData.clearDepth = true;
                cameraData.renderer = asset.scriptableRenderer;
                //cameraData.useScreenCoordOverride = false;
                // 桥接
                CameraDataUtils.SetUseScreenCoordOverride(ref cameraData, false);
            } else if (additionalCameraData != null) {
                cameraData.renderType = additionalCameraData.renderType;
                cameraData.clearDepth = (additionalCameraData.renderType != CameraRenderType.Base) ? additionalCameraData.clearDepth : true;
                //cameraData.useScreenCoordOverride = additionalCameraData.useScreenCoordOverride;
                // 桥接
                CameraDataUtils.SetUseScreenCoordOverride(ref cameraData, additionalCameraData.useScreenCoordOverride);
                cameraData.renderer = additionalCameraData.scriptableRenderer;
            } else {
                cameraData.renderType = CameraRenderType.Base;
                cameraData.clearDepth = true;
                //cameraData.useScreenCoordOverride = false;
                // 桥接
                CameraDataUtils.SetUseScreenCoordOverride(ref cameraData, false);
                cameraData.renderer = asset.scriptableRenderer;
            }

            //Matrix4x4 projectionMatrix = camera.projectionMatrix;
            //cameraData.SetViewAndProjectionMatrix(camera.worldToCameraMatrix, projectionMatrix);
            // 桥接
            CameraDataUtils.SetViewAndProjectionMatrix(ref cameraData, camera.worldToCameraMatrix, camera.projectionMatrix);

            cameraData.worldSpaceCameraPos = camera.transform.position;

            var backgroundColorSRGB = camera.backgroundColor;
            // Get the background color from preferences if preview camera
#if UNITY_EDITOR
            if (camera.cameraType == CameraType.Preview && camera.clearFlags != CameraClearFlags.SolidColor) {
                backgroundColorSRGB = CoreRenderPipelinePreferences.previewBackgroundColor;
            }
#endif
            cameraData.backgroundColor = CoreUtils.ConvertSRGBToActiveColorSpace(backgroundColorSRGB);
        }

        static void InitializeRenderingData(FunnyRenderPipelineAsset settings, ref CameraData cameraData, ref CullingResults cullResults,
            bool anyPostProcessingEnabled, CommandBuffer cmd, out RenderingData renderingData) {
            using var profScope = new ProfilingScope(null, Profiling.Pipeline.initializeRenderingData);

            /// 所有灯光
            var visibleLights = cullResults.visibleLights;
            //int mainLightIndex = UniversalRenderPipeline.GetMainLightIndex(settings, visibleLights);

            /// Temporary
            renderingData = new RenderingData();

            renderingData.cullResults = cullResults;
            renderingData.cameraData = cameraData;
            //renderingData.commandBuffer = cmd;
            // 桥接
            RenderingDataUtils.SetCommandBuffer(ref renderingData, cmd);
        }



        Comparison<Camera> cameraComparison = (camera1, camera2) => { return (int)camera1.depth - (int)camera2.depth; };
#if UNITY_2021_1_OR_NEWER
        /// <summary>
        /// 根据摄像机在摄像机渲染顺序中的深度进行排序
        /// </summary>
        void SortCameras(List<Camera> cameras) {
            if (cameras.Count > 1)
                cameras.Sort(cameraComparison);
        }

#else
        /// <summary>
        /// 根据摄像机在摄像机渲染顺序中的深度进行排序
        /// </summary>
        void SortCameras(Camera[] cameras){
            if (cameras.Length > 1)
                Array.Sort(cameras, cameraComparison);
        }

#endif

        static RenderTextureDescriptor CreateRenderTextureDescriptor(Camera camera) {
            RenderTextureDescriptor descriptor;

            if (camera.targetTexture == null) {
                descriptor = new RenderTextureDescriptor(camera.pixelWidth, camera.pixelHeight);

            } else {
                descriptor = camera.targetTexture.descriptor;
            }

            return descriptor;
        }

        /// <summary>
        /// 获得视锥剔除数据的参数 culling results
        /// </summary>
        static bool TryGetCullingParameters(CameraData cameraData, out ScriptableCullingParameters cullingParams) {
            return cameraData.camera.TryGetCullingParameters(false, out cullingParams);
        }

        /// <summary>
        /// 性能分析Profiling类
        /// </summary>
        private static class Profiling {
            private static Dictionary<int, ProfilingSampler> s_HashSamplerCache = new Dictionary<int, ProfilingSampler>();
            public static readonly ProfilingSampler unknownSampler = new ProfilingSampler("Unknown");

            // Specialization for camera loop to avoid allocations.
            public static ProfilingSampler TryGetOrAddCameraSampler(Camera camera) {
#if UNIVERSAL_PROFILING_NO_ALLOC
                return unknownSampler;
#else
                ProfilingSampler ps = null;
                int cameraId = camera.GetHashCode();
                bool exists = s_HashSamplerCache.TryGetValue(cameraId, out ps);
                if (!exists) {
                    // NOTE: camera.name allocates!
                    ps = new ProfilingSampler($"{nameof(FunnyRenderPipeline)}.{nameof(RenderSingleCameraInternal)}: {camera.name}");
                    s_HashSamplerCache.Add(cameraId, ps);
                }
                return ps;
#endif
            }

            public static class Pipeline {
#if UNITY_2021_1_OR_NEWER
                public static readonly ProfilingSampler beginContextRendering = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginContextRendering)}");
                public static readonly ProfilingSampler endContextRendering = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndContextRendering)}");
#else
                public static readonly ProfilingSampler beginFrameRendering = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginFrameRendering)}");
                public static readonly ProfilingSampler endFrameRendering = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndFrameRendering)}");
#endif
                public static readonly ProfilingSampler beginCameraRendering = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(BeginCameraRendering)}");
                public static readonly ProfilingSampler endCameraRendering = new ProfilingSampler($"{nameof(RenderPipeline)}.{nameof(EndCameraRendering)}");

                const string k_Name = nameof(FunnyRenderPipeline);
                public static readonly ProfilingSampler initializeCameraData = new ProfilingSampler($"{k_Name}.{nameof(InitializeCameraData)}");
                public static readonly ProfilingSampler initializeStackedCameraData = new ProfilingSampler($"{k_Name}.{nameof(InitializeStackedCameraData)}");
                public static readonly ProfilingSampler initializeAdditionalCameraData = new ProfilingSampler($"{k_Name}.{nameof(InitializeAdditionalCameraData)}");
                public static readonly ProfilingSampler initializeRenderingData = new ProfilingSampler($"{k_Name}.{nameof(InitializeRenderingData)}");

                public static class Renderer {
                    const string k_Name = nameof(ScriptableRenderer);
                    public static readonly ProfilingSampler setupCullingParameters = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderer.SetupCullingParameters)}");
                    public static readonly ProfilingSampler setup = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderer.Setup)}");
                };

                public static class Context {
                    const string k_Name = nameof(ScriptableRenderContext);
                    public static readonly ProfilingSampler submit = new ProfilingSampler($"{k_Name}.{nameof(ScriptableRenderContext.Submit)}");
                };
            };
        }
    }
}
