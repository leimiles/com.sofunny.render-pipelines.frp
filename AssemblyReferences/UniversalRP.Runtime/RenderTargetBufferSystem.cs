using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

namespace UnityEngine.Rendering.SoFunny {
    public class RenderTargetBufferSystemUtils {
        static RenderTargetBufferSystem m_RenderTargetBufferSystem;
        public RenderTargetBufferSystemUtils(string name) {
            m_RenderTargetBufferSystem = new RenderTargetBufferSystem(name);
        }
    }
}
