using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.Rendering.SoFunny {
    public static class RenderingDataUtils {
        public static void SetCommandBuffer(ref RenderingData renderingData, CommandBuffer commandBuffer) {
            renderingData.commandBuffer = commandBuffer;
        }
        public static CommandBuffer GetCommandBuffer(ref RenderingData renderingData) {
            return renderingData.commandBuffer;
        }
    }
    public static class CameraDataUtils {
        public static void SetPixelRect(ref CameraData cameraData, Rect rect) {
            cameraData.pixelRect = rect;
        }
        public static void SetPixelWidth(ref CameraData cameraData, int width) {
            cameraData.pixelWidth = width;
        }
        public static void SetPixelHeight(ref CameraData cameraData, int height) {
            cameraData.pixelHeight = height;
        }
        public static void SetAspectRatio(ref CameraData cameraData) {
            cameraData.aspectRatio = (float)cameraData.pixelWidth / (float)cameraData.pixelHeight;
        }
        public static void DisableXR(ref CameraData cameraData) {
            cameraData.xr = XRSystem.emptyPass;
        }
        public static void SetUseScreenCoordOverride(ref CameraData cameraData, bool value) {
            cameraData.useScreenCoordOverride = value;
        }
        public static void SetViewAndProjectionMatrix(ref CameraData cameraData, Matrix4x4 worldToCamera, Matrix4x4 projectionMatrix) {
            cameraData.SetViewAndProjectionMatrix(worldToCamera, projectionMatrix);
        }
    }

    public static class UniversalRenderPipelineCoreUtils {

    }
}
