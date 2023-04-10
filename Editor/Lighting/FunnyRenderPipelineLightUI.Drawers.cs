using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Rendering.SoFunny {
    using CED = CoreEditorDrawer<FunnyRenderPipelineSerializedLight>;
    internal partial class FunnyRenderPipelineLightUI {
        enum Expandable {
            General = 1 << 0
        }
        static readonly ExpandedState<Expandable, Light> k_ExpandedStatePreset = new(0, "FRP-preset");
        private static class Styles {
            public static readonly GUIContent DisabledLightWarning = EditorGUIUtility.TrTextContent("Lighting has been disabled in at least one Scene view. Any changes applied to lights in the Scene will not be updated in these views until Lighting has been enabled again.");
        }

        static void DrawGeneralContentPreset(FunnyRenderPipelineSerializedLight serializedLight, Editor owner) {
            DrawGeneralContentInternal(serializedLight, owner, isInPreset: false);
        }

        static void DrawGeneralContent(FunnyRenderPipelineSerializedLight serializedLight, Editor owner) {
            DrawGeneralContentInternal(serializedLight, owner, isInPreset: false);
        }

        static void DrawGeneralContentInternal(FunnyRenderPipelineSerializedLight serializedLight, Editor owner, bool isInPreset) {
            GUILayout.Label("此处定义物理光照参数");
        }
        static readonly ExpandedState<Expandable, Light> k_ExpandedState = new(~-1, "FRP");
        public static readonly CED.IDrawer PresetInspector = CED.Group(
            CED.Group((serialized, owner) =>
                EditorGUILayout.HelpBox(LightUI.Styles.unsupportedPresetPropertiesMessage, MessageType.Info)),
                            CED.Group((serialized, owner) => EditorGUILayout.Space()),
            CED.FoldoutGroup(LightUI.Styles.generalHeader, Expandable.General, k_ExpandedStatePreset, DrawGeneralContentPreset)
        );

        public static readonly CED.IDrawer Inspector = CED.Group(
            CED.Conditional(
                (_, __) => {
                    if (SceneView.lastActiveSceneView == null)
                        return false;

#if UNITY_2019_1_OR_NEWER
                    var sceneLighting = SceneView.lastActiveSceneView.sceneLighting;
#else
                    var sceneLighting = SceneView.lastActiveSceneView.m_SceneLighting;
#endif
                    return !sceneLighting;
                },
                (_, __) => EditorGUILayout.HelpBox(Styles.DisabledLightWarning.text, MessageType.Warning)),
            CED.FoldoutGroup(LightUI.Styles.generalHeader, Expandable.General, k_ExpandedState, DrawGeneralContent)
        );
    }
}
