using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.SoFunny {
    public static class RenderingUtils {
        public static DrawingSettings CreateDrawingSettings(List<ShaderTagId> shaderTagIdList, ref RenderingData renderingData, SortingCriteria sortingCriteria) {
            return UnityEngine.Rendering.Universal.RenderingUtils.CreateDrawingSettings(shaderTagIdList, ref renderingData, sortingCriteria);
        }

        public static void RenderObjectsWithError(ScriptableRenderContext context, ref CullingResults cullResults, Camera camera, FilteringSettings filterSettings, SortingCriteria sortFlags) {
            UnityEngine.Rendering.Universal.RenderingUtils.RenderObjectsWithError(context, ref cullResults, camera, filterSettings, sortFlags);
        }
    }
}
