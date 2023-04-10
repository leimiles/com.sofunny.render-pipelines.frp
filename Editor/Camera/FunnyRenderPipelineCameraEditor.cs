using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.SoFunny;
using UnityEditorInternal;

namespace UnityEditor.Rendering.SoFunny {
    using Styles = FunnylRenderPipelineCameraUI.Styles;

    [CustomEditorForRenderPipeline(typeof(Camera), typeof(FunnyRenderPipelineAsset))]
    [CanEditMultipleObjects]
    public class FunnyRenderPipelineCameraEditor : CameraEditor {
        List<(Camera, FunnyRenderPipelineSerializedCamera)> m_OutputWarningCameras = new();
        FunnyRenderPipelineSerializedCamera m_SerializedCamera;
        public new void OnEnable() {
            base.OnEnable();
            settings.OnEnable();
            //selectedCameraInStack = null;
            m_SerializedCamera = new FunnyRenderPipelineSerializedCamera(serializedObject, settings);
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
        }
    }
}
