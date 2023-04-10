using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.SoFunny;
using UnityEditor.Rendering.Universal;
using UnityEditor.SceneManagement;
using UnityEditorInternal;

namespace UnityEditor.Rendering.SoFunny {
    using Styles = FunnylRenderPipelineCameraUI.Styles;

    [CustomEditorForRenderPipeline(typeof(Camera), typeof(FunnyRenderPipelineAsset))]
    [CanEditMultipleObjects]
    public class FunnyRenderPipelineCameraEditor : CameraEditor {
        // Unity GUI工具 快速创建可排序的列表
        ReorderableList m_LayerList;
        List<Camera> validCameras = new List<Camera>();
        List<Camera> m_TypeErrorCameras = new List<Camera>();
        List<Camera> m_NotSupportedOverlayCameras = new List<Camera>();
        List<Camera> m_IncompatibleCameras = new List<Camera>();
        List<(Camera, FunnyRenderPipelineSerializedCamera)> m_OutputWarningCameras = new();
        FunnyRenderPipelineSerializedCamera m_SerializedCamera;
        public new void OnEnable() {
            base.OnEnable();
            settings.OnEnable();
            //selectedCameraInStack = null;
            m_SerializedCamera = new FunnyRenderPipelineSerializedCamera(serializedObject, settings);

            validCameras.Clear();
            m_TypeErrorCameras.Clear();
            m_NotSupportedOverlayCameras.Clear();
            m_IncompatibleCameras.Clear();
            m_OutputWarningCameras.Clear();

            UpdateCameras();

            Undo.undoRedoPerformed += ReconstructReferenceToAdditionalDataSO;
        }
        void ReconstructReferenceToAdditionalDataSO() {
            OnDisable();
            OnEnable();
        }

        public new void OnDisable() {
            base.OnDisable();
            Undo.undoRedoPerformed -= ReconstructReferenceToAdditionalDataSO;
        }
        
        void UpdateCameras() {
            m_SerializedCamera.Refresh();

            // UI
            // m_LayerList = new ReorderableList(m_SerializedCamera.serializedObject, m_SerializedCamera.cameras, true, true, true, true) {
            //     drawHeaderCallback = rect => EditorGUI.LabelField(rect, Styles.cameras),
            //     drawElementCallback = DrawElementCallback,
            //     onSelectCallback = SelectElement,
            //     onRemoveCallback = RemoveCamera,
            //     onCanRemoveCallback = CanRemoveCamera,
            //     onAddDropdownCallback = AddCameraToCameraList
            // };
        }

        /// <summary>
        /// 判断相机是否可以删除
        /// </summary>
        bool CanRemoveCamera(ReorderableList list) => m_SerializedCamera.numCameras > 0;

        /// <summary>
        /// 用于绘制每个相机的信息
        /// </summary>
        void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused) {
        }

        /// <summary>
        /// 选择相机
        /// </summary>
        void SelectElement(ReorderableList list) {
        }

        /// <summary>
        /// 删除相机时回调，用于清空相机信息
        /// </summary>
        void RemoveCamera(ReorderableList list) {
        }

        /// <summary>
        /// 添加相机
        /// </summary>
        void AddCameraToCameraList(Rect rect, ReorderableList list) {
        }
    }
}
