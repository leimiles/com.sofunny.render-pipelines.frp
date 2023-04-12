using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Serialization;
using UnityEngine.Assertions;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.SoFunny {
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    [ImageEffectAllowedInSceneView]
    public class FunnyAdditionalCameraData : MonoBehaviour, ISerializationCallbackReceiver, IAdditionalData {
        [SerializeField] CameraRenderType m_CameraType = CameraRenderType.Base;
        [SerializeField] List<Camera> m_Cameras = new List<Camera>();
        [SerializeField] int m_RendererIndex = -1;
        [SerializeField] bool m_RenderPostProcessing = false;

        [SerializeField] bool m_ClearDepth = true;

        [SerializeField] bool m_UseScreenCoordOverride;

        [NonSerialized] Camera m_Camera;
        static FunnyAdditionalCameraData s_DefaultAdditionalCameraData = null;
        internal static FunnyAdditionalCameraData defaultAdditionalCameraData {
            get {
                if (s_DefaultAdditionalCameraData == null)
                    s_DefaultAdditionalCameraData = new FunnyAdditionalCameraData();

                return s_DefaultAdditionalCameraData;
            }
        }

#if UNITY_EDITOR
        internal new Camera camera
#else
        internal Camera camera
#endif
        {
            get {
                if (!m_Camera) {
                    gameObject.TryGetComponent<Camera>(out m_Camera);
                }
                return m_Camera;
            }
        }

        /// <summary>
        /// Returns the camera renderType.
        /// <see cref="CameraRenderType"/>.
        /// </summary>
        public CameraRenderType renderType {
            get => m_CameraType;
            set => m_CameraType = value;
        }

        /// <summary>
        /// If true, this camera will clear depth value before rendering. Only valid for Overlay cameras.
        /// </summary>
        public bool clearDepth {
            get => m_ClearDepth;
        }

        /// <summary>
        /// Returns true if the camera uses Screen Coordinates Override.
        /// </summary>
        public bool useScreenCoordOverride {
            get => m_UseScreenCoordOverride;
            set => m_UseScreenCoordOverride = value;
        }
        /// <summary>
        /// Returns the camera stack. Only valid for Base cameras.
        /// Will return null if it is not a Base camera.
        /// </summary>
        public List<Camera> cameraStack {
            get {
                if (renderType != CameraRenderType.Base) {
                    var camera = gameObject.GetComponent<Camera>();
                    Debug.LogWarning(string.Format("{0}: This camera is of {1} type. Only Base cameras can have a camera stack.", camera.name, renderType));
                    return null;
                }

                if (!scriptableRenderer.SupportsCameraStackingType(CameraRenderType.Base)) {
                    var camera = gameObject.GetComponent<Camera>();
                    Debug.LogWarning(string.Format("{0}: This camera has a ScriptableRenderer that doesn't support camera stacking. Camera stack is null.", camera.name));
                    return null;
                }
                return m_Cameras;
            }
        }

        /// <summary>
        /// Returns the <see cref="ScriptableRenderer"/> that is used to render this camera.
        /// </summary>
        public ScriptableRenderer scriptableRenderer {
            get {
                if (FunnyRenderPipeline.asset is null)
                    return null;
                if (!FunnyRenderPipeline.asset.ValidateRendererData(m_RendererIndex)) {
                    int defaultIndex = FunnyRenderPipeline.asset.m_DefaultRendererIndex;
                    Debug.LogWarning(
                        $"Renderer at <b>index {m_RendererIndex.ToString()}</b> is missing for camera <b>{camera.name}</b>, falling back to Default Renderer. <b>{FunnyRenderPipeline.asset.m_RendererDataList[defaultIndex].name}</b>",
                        FunnyRenderPipeline.asset);
                    return FunnyRenderPipeline.asset.GetRenderer(defaultIndex);
                }
                return FunnyRenderPipeline.asset.GetRenderer(m_RendererIndex);
            }
        }

        public void SetRenderer(int index) {
            m_RendererIndex = index;
        }

        /// <summary>
        /// Returns true if this camera should render post-processing.
        /// </summary>
        public bool renderPostProcessing {
            get => m_RenderPostProcessing;
            set => m_RenderPostProcessing = value;
        }

        /// <inheritdoc/>
        public void OnBeforeSerialize() {
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize() {
        }
    }
}
