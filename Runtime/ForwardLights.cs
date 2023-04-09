using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.SoFunny {
    public class ForwardLights {
        const string k_SetupLightConstants = "Setup Light Constants";
        private static readonly ProfilingSampler m_ProfilingSampler = new ProfilingSampler(k_SetupLightConstants);

        internal void PreSetup(ref RenderingData renderingData) {
            // ...
        }

        public void Setup(ScriptableRenderContext context, ref RenderingData renderingData) {
            var cmd = RenderingDataUtils.GetCommandBuffer(ref renderingData);
            using (new ProfilingScope(null, m_ProfilingSampler)) {
                // todo, setup forward plus data first
                // ...


                SetupShaderLightConstants(cmd, ref renderingData);
            }
        }
        void SetupShaderLightConstants(CommandBuffer cmd, ref RenderingData renderingData) {
            SetupMainLightConstants(cmd, ref renderingData.lightData);
        }

        void SetupMainLightConstants(CommandBuffer cmd, ref LightData lightData) {
            InitializeLightConstants(lightData.visibleLights, lightData.mainLightIndex);
        }

        void InitializeLightConstants(NativeArray<VisibleLight> lights, int lightIndex) {
            if (lightIndex < 0) {
                return;
            }

            ref VisibleLight visibleLights = ref lights.UnsafeElementAtMutable(lightIndex);

            Light light = visibleLights.light;
            if (light == null) {
                return;
            }
            var additionalLightData = light.GetFunnyAdditionalLightData();
        }
    }
}
