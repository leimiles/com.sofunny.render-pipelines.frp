using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.SoFunny {
    public partial class FunnyRenderPipelineAsset : RenderPipelineAsset, ISerializationCallbackReceiver {

        protected override RenderPipeline CreatePipeline() {
            throw new System.NotImplementedException();
        }

        public void OnAfterDeserialize() {
            throw new System.NotImplementedException();
        }

        public void OnBeforeSerialize() {
            throw new System.NotImplementedException();
        }

    }
}
