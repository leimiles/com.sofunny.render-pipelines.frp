
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
#endif
namespace UnityEngine.Rendering.SoFunny {

    public enum RendererType {
        /// <summary>
        /// Use this for Custom Renderer.
        /// </summary>
        Custom,

        /// <summary>
        /// Use this for Universal Renderer.
        /// </summary>
        FunnyRenderer,

        /// <summary>
        /// Use this for 2D Renderer.
        /// </summary>
        _2DRenderer,
        // /// <summary>
        // /// This name was used before the Universal Renderer was implemented.
        // /// </summary>
        // [Obsolete("ForwardRenderer has been renamed (UnityUpgradable) -> UniversalRenderer", true)]
        // ForwardRenderer = UniversalRenderer,
    }

    public class FunnyRenderPipelineAsset : RenderPipelineAsset {
        ScriptableRenderer[] m_Renderers = new ScriptableRenderer[1];


        [SerializeField] bool m_SupportsHDR = true;
        internal int m_DefaultRendererIndex = 0;
        [SerializeField] internal ScriptableRendererData[] m_RendererDataList = new ScriptableRendererData[1];

        /// <summary>
        /// 返回渲染管线的实例，渲染管线会按照该实例的 Render() 函数安排渲染流程
        /// </summary>
        protected override RenderPipeline CreatePipeline() {
            var pipeline = new FunnyRenderPipeline(this);
            CreateRenderers();
            return pipeline;
        }

        /// <summary>
        /// 返回Renderer实例
        /// </summary>
        void CreateRenderers() {
            if (m_Renderers == null) {
                m_Renderers = new ScriptableRenderer[1];
            }

            if (m_Renderers == null || m_Renderers.Length != m_RendererDataList.Length)
                m_Renderers = new ScriptableRenderer[m_RendererDataList.Length];

            for (int i = 0; i < m_RendererDataList.Length; ++i) {
                if (m_RendererDataList[i] != null) {
                    m_Renderers[m_DefaultRendererIndex] = m_RendererDataList[i].InternalCreateRenderer();
                }
            }
        }
#if UNITY_EDITOR
        /// <summary>
        // 在菜单中创建Asset
        /// </summary>
        [MenuItem("Assets/Create/Rendering/Funny Prender Pipeline", priority = CoreUtils.Sections.section2 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 1)]
        static void CreateFunnyRenderPipeline() {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateFunnyPipelineAsset>(),
                "Funny Render Pipeline Asset.asset", null, null);
        }


        /// <summary>
        // 创建渲染管线 asset 文件，并允许重命名
        /// </summary>
        internal class CreateFunnyPipelineAsset : EndNameEditAction {
            // pathName 会传递当前 ScriptableObject 的路径，基于该路径设置 renderer asset 的位置
            public override void Action(int instanceId, string pathName, string resourceFile) {
                // 创建渲染管线 asset 文件，同时创建对应的 renderer asset 文件
                AssetDatabase.CreateAsset(Create(CreateRendererAsset(pathName, RendererType.FunnyRenderer)), pathName);
            }
        }


        /// <summary>
        /// 创建渲染管线 asset 实例并返回，默认情况下，还会同时创建 renderer 的 asset 文件，配置给渲染管线后，返回其实例
        /// </summary>
        public static FunnyRenderPipelineAsset Create(ScriptableRendererData rendererData = null) {
            var instance = CreateInstance<FunnyRenderPipelineAsset>();
            if (rendererData != null) {
                instance.m_RendererDataList[0] = rendererData;
            } else {
                instance.m_RendererDataList[0] = CreateInstance<FunnyRenderData>(); //////////// 是否重写RenderData？？？？？？
            }
            return instance;
        }


        /// <summary>
        /// 创建对应的 renderer asset 文件
        /// </summary>
        internal static FunnyRenderData CreateRendererAsset(string path, RendererType type, bool relativePath = true, string suffix = "Renderer") {
            FunnyRenderData funnyRendererData = CreateRendererData(type);
            string dataPath;
            if (relativePath) {
                dataPath =
                $"{Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path))}_{suffix}{Path.GetExtension(path)}";
            } else {
                dataPath = path;
            }
            AssetDatabase.CreateAsset(funnyRendererData, dataPath);
            return funnyRendererData;
        }

        /// <summary>
        /// 创建不同的PipelineRenderData信息
        /// </summary>
        static FunnyRenderData CreateRendererData(RendererType type) {
            switch (type) {
                case RendererType.FunnyRenderer:
                default: {
                        var rendererData = CreateInstance<FunnyRenderData>();
                        return rendererData;
                    }
                    // case RendererType._2DRenderer: {
                    //     var rendererData = CreateInstance<Renderer2DData>();
                    //     rendererData.postProcessData = PostProcessData.GetDefaultPostProcessData();
                    //     return rendererData;
                    // }
            }
        }
#endif

        internal void DestroyRenderers() {
            if (m_Renderers == null)
                return;

            for (int i = 0; i < m_Renderers.Length; i++)
                DestroyRenderer(ref m_Renderers[i]);
        }

        void DestroyRenderer(ref ScriptableRenderer renderer) {
            if (renderer != null) {
                renderer.Dispose();
                renderer = null;
            }
        }

        /// <summary>
        /// 加载或者实例化的时候调用
        /// </summary>
        protected override void OnValidate() {
            DestroyRenderers();

            // This will call RenderPipelineManager.CleanupRenderPipeline that in turn disposes the render pipeline instance and
            // assign pipeline asset reference to null
            base.OnValidate();
        }

        /// <summary>
        /// 资产被删除时调用
        /// </summary>
        protected override void OnDisable() {
            DestroyRenderers();

            // This will call RenderPipelineManager.CleanupRenderPipeline that in turn disposes the render pipeline instance and
            // assign pipeline asset reference to null
            base.OnDisable();
        }


        /// <summary>
        /// 返回当前Render的实例
        /// UniversalAdditionalCameraData 脚本会调用 但是现在调用的是URP的
        /// </summary>
        public ScriptableRenderer GetRenderer(int index) {
            return m_Renderers[index];
        }



        internal ScriptableRendererData scriptableRendererData {
            get {
                if (m_RendererDataList[m_DefaultRendererIndex] == null)
                    CreatePipeline();

                return m_RendererDataList[m_DefaultRendererIndex];
            }
        }

        public ScriptableRenderer scriptableRenderer {
            get {
                if (m_RendererDataList?.Length > m_DefaultRendererIndex && m_RendererDataList[m_DefaultRendererIndex] == null) {
                    Debug.LogError("Default renderer is missing from the current Pipeline Asset.", this);
                    return null;
                }

                if (scriptableRendererData.isInvalidated || m_Renderers[m_DefaultRendererIndex] == null) {
                    DestroyRenderer(ref m_Renderers[m_DefaultRendererIndex]);
                    m_Renderers[m_DefaultRendererIndex] = scriptableRendererData.InternalCreateRenderer();
                }

                return m_Renderers[m_DefaultRendererIndex];
            }
        }

        /// <summary>
        /// 是否开启HDR渲染
        /// </summary>
        public bool supportsHDR {
            get { return m_SupportsHDR; }
            set { m_SupportsHDR = value; }
        }

        public void OnAfterDeserialize() {
        }

        public void OnBeforeSerialize() {
        }

        internal bool ValidateRendererData(int index) {
            // Check to see if you are asking for the default renderer
            if (index == -1) index = m_DefaultRendererIndex;
            return index < m_RendererDataList.Length ? m_RendererDataList[index] != null : false;
        }

    }
}
