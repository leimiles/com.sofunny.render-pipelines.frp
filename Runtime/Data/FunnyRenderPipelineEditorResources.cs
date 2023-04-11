using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace UnityEngine.Rendering.SoFunny {
    public class FunnyRenderPipelineEditorResources : ScriptableObject {
        /// <summary>
        /// 默认 shader 文件所在位置
        /// </summary>
        [Serializable, ReloadGroup]
        public sealed class ShaderResources {
            /// <summary>
            /// 默认的 test shader，untiy 默认
            /// </summary>
            [Reload("Shaders/Test.shader")]
            public Shader test;
            [Reload("Shaders/UI-Default.shader")]
            public Shader uiDefault;
        }

        /// <summary>
        /// 默认 material 文件所在位置
        /// </summary>
        [Serializable, ReloadGroup]
        public sealed class MaterialResources {
            [Reload("Runtime/Materials/Test.mat")]
            public Material test;
            [Reload("Runtime/Materials/UI-Default.mat")]
            public Material uiDefault;
        }

        public ShaderResources shaderResources;
        public MaterialResources materialResources;
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(FunnyRenderPipelineEditorResources), true)]
    class UniversalRenderPipelineEditorResourcesEditor : UnityEditor.Editor {
        /// <inheritdoc/>
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            // Add a "Reload All" button in inspector when we are in developer's mode
            if (UnityEditor.EditorPrefs.GetBool("DeveloperMode") && GUILayout.Button("Reload All")) {
                var resources = target as FunnyRenderPipelineEditorResources;
                resources.materialResources = null;
                resources.shaderResources = null;
                ResourceReloader.ReloadAllNullIn(target, FunnyRenderPipelineAsset.packagePath);
            }
        }
    }
#endif
}