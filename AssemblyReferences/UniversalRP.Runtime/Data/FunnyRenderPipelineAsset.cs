using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using System.IO;
using UnityEditorInternal;
using ShaderKeywordFilter = UnityEditor.ShaderKeywordFilter;
#endif

namespace UnityEngine.Rendering.SoFunny {

    public enum RendererType {
        Custom,
        FunnyRenderer,
        _2DRenderer,
    }

    internal enum DefaultMaterialType {
        Unlit
    }

    public class FunnyRenderPipelineAsset : RenderPipelineAsset, ISerializationCallbackReceiver {
        Shader m_DefaultShader;
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
        [MenuItem("Assets/Create/SoFunny Rendering/FRP Asset (with Funny Renderer)", priority = CoreUtils.Sections.section2 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority + 1)]
        static void CreateFunnyRenderPipeline() {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateFunnyPipelineAsset>(),
                "Funny Render Pipeline Asset.asset", null, null);
        }

        [NonSerialized]
        internal FunnyRenderPipelineEditorResources m_EditorResourcesAsset;
        public static readonly string editorResourcesGUID = "e3f5a85ebd6e3474eb74e9483bc0c868";

        /// <summary>
        // 创建默认的 editor resources，开发完后关闭可见
        /// </summary>
        //[MenuItem("Assets/Create/SoFunny Rendering/FRP Editor Resources", priority = CoreUtils.Sections.section8 + CoreUtils.Priorities.assetsCreateRenderingMenuPriority)]
        static void CreateFunnyPipelineEditorResources() {
            var instance = CreateInstance<FunnyRenderPipelineEditorResources>();
            ResourceReloader.ReloadAllNullIn(instance, packagePath);
            AssetDatabase.CreateAsset(instance, string.Format("Assets/{0}.asset", typeof(FunnyRenderPipelineEditorResources).Name));
        }

        FunnyRenderPipelineEditorResources editorResources {
            get {
                if (m_EditorResourcesAsset != null && !m_EditorResourcesAsset.Equals(null)) {
                    return m_EditorResourcesAsset;
                }

                string resourcePath = AssetDatabase.GUIDToAssetPath(editorResourcesGUID);
                var objs = InternalEditorUtility.LoadSerializedFileAndForget(resourcePath);
                m_EditorResourcesAsset = objs != null && objs.Length > 0 ? objs.First() as FunnyRenderPipelineEditorResources : null;
                return m_EditorResourcesAsset;
            }
        }

        public static readonly string packagePath = "Packages/com.sofunny.render-pipelines.frp";

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
                instance.m_RendererDataList[0] = CreateInstance<FunnyRendererData>(); //////////// 是否重写RenderData？？？？？？
            }
            return instance;
        }


        /// <summary>
        /// 创建对应的 renderer asset 文件
        /// </summary>
        internal static FunnyRendererData CreateRendererAsset(string path, RendererType type, bool relativePath = true, string suffix = "Renderer") {
            FunnyRendererData funnyRendererData = CreateRendererData(type);
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
        static FunnyRendererData CreateRendererData(RendererType type) {
            switch (type) {
                case RendererType.FunnyRenderer:
                default: {
                        var rendererData = CreateInstance<FunnyRendererData>();
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

        /// <summary>
        /// 在场景中新建 gameobject 时使用的默认 material
        /// </summary>
        public override Material defaultMaterial {
            get {
                return GetMaterial(DefaultMaterialType.Unlit);
            }
        }

        /// <summary>
        /// 在工程中创建材质球时使用的默认 shader
        /// </summary>
        public override Shader defaultShader {
            get {
#if UNITY_EDITOR
                if (scriptableRendererData != null) {
                    Shader shader = scriptableRendererData.GetDefaultShader();
                    if (shader != null) {
                        return defaultShader;
                    }
                }
                if (m_DefaultShader == null) {
                    string path = AssetDatabase.GUIDToAssetPath(ShaderUtils.GetShaderGUID(ShaderPathID.Unlit));
                    m_DefaultShader = AssetDatabase.LoadAssetAtPath<Shader>(path);
                }
#endif
                if (m_DefaultShader == null) {
                    m_DefaultShader = Shader.Find(ShaderUtils.GetShaderPath(ShaderPathID.Unlit));
                }
                return m_DefaultShader;
            }
        }

        /// <summary>
        /// 根据不同材质类型获取的默认材质
        /// </summary>
        Material GetMaterial(DefaultMaterialType materialType) {
#if UNITY_EDITOR
            if (scriptableRendererData == null || editorResources == null) {
                return null;
            }

            switch (materialType) {
                case DefaultMaterialType.Unlit:
                    return editorResources.materialResources.unlit;
                default:
                    return null;

            }
#else
            return null;
#endif

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
