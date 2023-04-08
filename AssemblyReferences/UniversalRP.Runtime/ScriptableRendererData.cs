using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.SoFunny {
    public static class ScriptableRendererDataUtils {
        public static ScriptableRenderer InternalCreateRenderer(ScriptableRendererData scriptableRendererData) {
            return scriptableRendererData.InternalCreateRenderer();
        }
#if UNITY_EDITOR
        public static Shader GetDefaultShader(ScriptableRendererData scriptableRendererData) {
            return scriptableRendererData.GetDefaultShader();
        }
#endif

        public static bool IsInvalidated(ScriptableRendererData scriptableRendererData) {
            return scriptableRendererData.isInvalidated;
        }
    }
}
