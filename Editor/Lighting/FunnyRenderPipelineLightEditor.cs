using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.SoFunny;
using UnityEditorInternal;


namespace UnityEditor.Rendering.SoFunny {
    [CanEditMultipleObjects]
    [CustomEditorForRenderPipeline(typeof(Light), typeof(FunnyRenderPipelineAsset))]
    public class FunnyRenderPipelineLightEditor : LightEditor {
        FunnyRenderPipelineSerializedLight serializedLight { get; set; }
        protected override void OnEnable() {
            serializedLight = new FunnyRenderPipelineSerializedLight(serializedObject, settings);
            Undo.undoRedoPerformed += ReconstructReferenceToAdditionalDataSO;
        }

        protected void OnDisable() {
            Undo.undoRedoPerformed -= ReconstructReferenceToAdditionalDataSO;
        }

        internal static bool IsPresetEditor(UnityEditor.Editor editor) {
            return (int)((editor.target as Component).gameObject.hideFlags) == 93;
        }

        internal void ReconstructReferenceToAdditionalDataSO() {
            OnDisable();
            OnEnable();
        }

        public override void OnInspectorGUI() {
            serializedLight.Update();

            if (IsPresetEditor(this)) {
                FunnyRenderPipelineLightUI.PresetInspector.Draw(serializedLight, this);
            } else {
                FunnyRenderPipelineLightUI.Inspector.Draw(serializedLight, this);
            }

            serializedLight.Apply();
        }

        protected override void OnSceneGUI() {
            if (!(GraphicsSettings.currentRenderPipeline is FunnyRenderPipelineAsset))
                return;

            if (!(target is Light light) || light == null)
                return;
        }

    }
}
