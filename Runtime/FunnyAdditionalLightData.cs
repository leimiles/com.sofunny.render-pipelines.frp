using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Rendering.SoFunny {
    public static class LightExtensions {
        public static FunnyAdditionalLightData GetFunnyAdditionalLightData(this Light light) {
            var gameObject = light.gameObject;
            bool componentExists = gameObject.TryGetComponent<FunnyAdditionalLightData>(out var lightData);
            if (!componentExists) {
                lightData = gameObject.AddComponent<FunnyAdditionalLightData>();
            }
            return lightData;
        }
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public class FunnyAdditionalLightData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData {
        public void OnAfterDeserialize() {
        }

        public void OnBeforeSerialize() {
        }

    }
}
