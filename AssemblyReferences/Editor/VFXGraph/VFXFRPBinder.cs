using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.VFX;
using System;

namespace UnityEditor.Rendering.SoFunny {
    internal class VFXFRPBinder : VFXSRPBinder {
        public override string templatePath { get { return "Packages/com.unity.render-pipelines.universal/Editor/VFXGraph/Shaders"; } }
        public override string runtimePath { get { return "Packages/com.unity.render-pipelines.universal/Runtime/VFXGraph/Shaders"; } }
        public override string SRPAssetTypeStr { get { return "FunnyRenderPipelineAsset"; } }
        public override Type SRPOutputDataType { get { return null; } } // null by now but use VFXURPSubOutput when there is a need to store URP specific data
    }
}
