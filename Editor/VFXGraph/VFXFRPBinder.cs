using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.VFX;
using System;

namespace UnityEditor.Rendering.SoFunny {
    class FRPVFXBinder : VFXSRPBinder {
        public override string templatePath {
            get {
                return "Packages/com.unity.render-pipelines.frp/Editor/VFXGraph/Shaders";
            }
        }
        public override string runtimePath {
            get {
                return "Packages/com.unity.render-pipelines.frp/Runtime/VFXGraph/Shaders";
            }
        }

        public override string SRPAssetTypeStr {
            get {
                return "FunnyRenderPipelineAsset";
            }
        }

        public override Type SRPOutputDataType {
            get {
                return null;
            }
        }
    }
}