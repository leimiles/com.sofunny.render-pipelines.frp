using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.SoFunny {
    public static class ScriptableRendererUtils {
        public static void SetRenderer(ScriptableRenderer scriptableRenderer) {
            ScriptableRenderer.current = scriptableRenderer;
        }

        public static void SetRendererNull() {
            ScriptableRenderer.current = null;
        }

        public static bool IsUseDepthPriming(ScriptableRenderer scriptableRenderer) {
            return scriptableRenderer.useDepthPriming;
        }

        public static void Clear(ScriptableRenderer scriptableRenderer, CameraRenderType cameraRenderType) {
            scriptableRenderer.Clear(cameraRenderType);
        }

        public static void OnPreCullRenderPasses(ScriptableRenderer scriptableRenderer, in CameraData cameraData) {
            scriptableRenderer.OnPreCullRenderPasses(in cameraData);
        }

        public static void AddRenderPasses(ScriptableRenderer scriptableRenderer, ref RenderingData renderingData) {
            scriptableRenderer.AddRenderPasses(ref renderingData);
        }
    }
}
