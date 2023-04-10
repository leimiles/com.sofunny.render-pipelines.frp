using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Rendering;
using UnityEngine.Rendering.SoFunny;
using UnityEditor;

namespace UnityEditor.Rendering.SoFunny {
    public class FunnyRenderPipelineSerializedLight : ISerializedLight {
        public LightEditor.Settings settings { get; }

        public SerializedObject serializedObject { get; }

        public SerializedObject serializedAdditionalDataObject { get; private set; }

        public SerializedProperty intensity { get; }

        public FunnyAdditionalLightData[] lightsAddtionalData { get; private set; }

        public FunnyAdditionalLightData additionalLightData => lightsAddtionalData[0];

        public void Apply() {
            serializedObject.Update();
            serializedAdditionalDataObject.Update();
            settings.Update();
        }

        public void Update() {
            serializedObject.ApplyModifiedProperties();
            serializedAdditionalDataObject.ApplyModifiedProperties();
            settings.ApplyModifiedProperties();
        }

        public FunnyRenderPipelineSerializedLight(SerializedObject serializedObject, LightEditor.Settings settings) {
            this.settings = settings;
            settings.OnEnable();

            this.serializedObject = serializedObject;

            lightsAddtionalData = CoreEditorUtils.GetAdditionalData<FunnyAdditionalLightData>(serializedObject.targetObjects);
            serializedAdditionalDataObject = new SerializedObject(lightsAddtionalData);

            settings.ApplyModifiedProperties();
        }
    }
}
