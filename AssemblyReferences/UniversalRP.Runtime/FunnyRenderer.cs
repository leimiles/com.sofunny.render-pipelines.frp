using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.SoFunny {
    public class FunnyRenderer : ScriptableRenderer {
        public FunnyRenderer(ScriptableRendererData data) : base(data) {
        }

        public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData) {
            throw new System.NotImplementedException();
        }
    }
}