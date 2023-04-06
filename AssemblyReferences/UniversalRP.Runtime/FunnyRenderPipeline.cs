using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace UnityEngine.Rendering.SoFunny {
    public class FunnyRenderPipeline : RenderPipeline {
        private readonly FunnyRenderPipelineAsset pipelineAsset;
        public FunnyRenderPipeline(FunnyRenderPipelineAsset asset) {
            this.pipelineAsset = asset;
            RTHandles.Initialize(Screen.width, Screen.height);
        }

        // Start is called before the first frame update
        protected override void Render(ScriptableRenderContext context, Camera[] cameras) {
        }
    }
}
