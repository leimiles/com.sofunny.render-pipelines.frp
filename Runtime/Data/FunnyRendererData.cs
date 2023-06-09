using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.SoFunny {
    public class FunnyRendererData : ScriptableRendererData, ISerializationCallbackReceiver {
        [SerializeField] LayerMask m_OpaqueLayerMask = -1;
        [SerializeField] LayerMask m_TransparentLayerMask = -1;
        [SerializeField] StencilStateData m_DefaultStencilState = new StencilStateData() { passOperation = StencilOp.Replace }; // This default state is compatible with deferred renderer.

        /// <summary>
        /// 设置模板测试状态
        /// </summary>
        public StencilStateData defaultStencilState {
            get => m_DefaultStencilState;
            set {
                SetDirty();
                m_DefaultStencilState = value;
            }
        }

        /// <summary>
        /// 设置不透明对象的过滤
        /// </summary>
        public LayerMask opaqueLayerMask {
            get => m_OpaqueLayerMask;
            set {
                SetDirty();
                m_OpaqueLayerMask = value;

            }
        }

        /// <summary>
        /// 设置不透明对象的过滤
        /// </summary>
        public LayerMask transparentLayerMask {
            get => m_TransparentLayerMask;
            set {
                SetDirty();
                m_TransparentLayerMask = value;
            }
        }

        protected override ScriptableRenderer Create() {
            return new FunnyRenderer(this);
        }

        public void OnAfterDeserialize() {
        }

        public void OnBeforeSerialize() {
        }
    }
}
