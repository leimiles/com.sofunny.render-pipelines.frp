using System;
using UnityEngine;
using UnityEngine.Rendering.SoFunny;

namespace UnityEditor.Rendering.SoFunny {
    public class FunnyRenderPipelineSerializedCamera : ISerializedCamera {
        FunnyRenderPipelineSerializedCamera[] cameraSerializedObjects { get; set; }
        public FunnyAdditionalCameraData[] camerasAdditionalData { get; }
        public SerializedObject serializedObject { get; }
        public SerializedObject serializedAdditionalDataObject { get; }

        // Unity Base CameraData
        public CameraEditor.Settings baseCameraSettings { get; }

        // This one is internal in UnityEditor for whatever reason...
        public SerializedProperty projectionMatrixMode { get; }
        public SerializedProperty dithering { get; }
        public SerializedProperty stopNaNs { get; }
        public SerializedProperty allowDynamicResolution { get; }
        public SerializedProperty volumeLayerMask { get; }
        public SerializedProperty clearDepth { get; }
        public SerializedProperty antialiasing { get; }
        public SerializedProperty cameras { get; set; }
        public int numCameras => cameras?.arraySize ?? 0;

        /// 键值类型
        public (Camera camera, FunnyRenderPipelineSerializedCamera serializedCamera) this[int index] {
            get {
                if (index < 0 || index >= numCameras)
                    throw new ArgumentOutOfRangeException($"{index} is out of bounds [0 - {numCameras}]");

                // Return the camera on that index
                return (cameras.GetArrayElementAtIndex(index).objectReferenceValue as Camera, cameraSerializedObjects[index]);
            }
        }

        public FunnyRenderPipelineSerializedCamera(SerializedObject serializedObject, CameraEditor.Settings settings = null) {
            this.serializedObject = serializedObject;
            projectionMatrixMode = serializedObject.FindProperty("m_projectionMatrixMode");
            allowDynamicResolution = serializedObject.FindProperty("m_AllowDynamicResolution");

            if (settings == null) {
                baseCameraSettings = new CameraEditor.Settings(serializedObject);
                baseCameraSettings.OnEnable();
            } else {
                baseCameraSettings = settings;
            }

            // 获取序列化 AdditionData
            camerasAdditionalData = CoreEditorUtils
                .GetAdditionalData<FunnyAdditionalCameraData>(serializedObject.targetObjects);
            serializedAdditionalDataObject = new SerializedObject(camerasAdditionalData);

            // Common properties
            dithering = serializedAdditionalDataObject.FindProperty("m_Dithering");
            stopNaNs = serializedAdditionalDataObject.FindProperty("m_StopNaNs");
            volumeLayerMask = serializedAdditionalDataObject.FindProperty("m_VolumeLayerMask");
            clearDepth = serializedAdditionalDataObject.FindProperty("m_ClearDepth");
            antialiasing = serializedAdditionalDataObject.FindProperty("m_Antialiasing");

            //FRP properties
        }


        /// <summary>
        /// Updates the internal serialized objects
        /// </summary>
        public void Update() {
            baseCameraSettings.Update();
            serializedObject.Update();
            serializedAdditionalDataObject.Update();

            for (int i = 0; i < numCameras; ++i) {
                cameraSerializedObjects[i].Update();
            }
        }

        /// <summary>
        /// Applies the modified properties to the serialized objects
        /// </summary>
        public void Apply() {
            baseCameraSettings.ApplyModifiedProperties();
            serializedObject.ApplyModifiedProperties();
            serializedAdditionalDataObject.ApplyModifiedProperties();

            for (int i = 0; i < numCameras; ++i) {
                cameraSerializedObjects[i].Apply();
            }
        }

        /// <summary>
        /// Refreshes the serialized properties from the serialized objects
        /// </summary>
        public void Refresh() {
            var o = new PropertyFetcher<FunnyAdditionalCameraData>(serializedAdditionalDataObject);
            cameras = o.Find("m_Cameras");

            cameraSerializedObjects = new FunnyRenderPipelineSerializedCamera[numCameras];
            for (int i = 0; i < numCameras; ++i) {
                Camera cam = cameras.GetArrayElementAtIndex(i).objectReferenceValue as Camera;
                cameraSerializedObjects[i] = new FunnyRenderPipelineSerializedCamera(new SerializedObject(cam));
            }
        }
    }
}
