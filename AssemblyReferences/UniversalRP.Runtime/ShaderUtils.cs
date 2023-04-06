using System;
using System.Linq;

namespace UnityEngine.Rendering.SoFunny {
    public enum ShaderPathID {
        Unlit
    }
    public static class ShaderUtils {
        static readonly string[] s_ShaderPaths =
        {
            "So Funny/FRP/Unlit"
        };

        public static string GetShaderPath(ShaderPathID id) {
            int index = (int)id;
            int arrayLength = s_ShaderPaths.Length;
            if (arrayLength > 0 && index >= 0 && index < arrayLength)
                return s_ShaderPaths[index];

            Debug.LogError("Trying to access frp shader path out of bounds: (" + id + ": " + index + ")");
            return "";
        }


#if UNITY_EDITOR
        static readonly string[] s_ShaderGUIDs =
        {
            "f98ad0c5198aaa345b02b4921d4e6597"
        };
        public static string GetShaderGUID(ShaderPathID id) {
            int index = (int)id;
            int arrayLength = s_ShaderGUIDs.Length;
            if (arrayLength > 0 && index >= 0 && index < arrayLength)
                return s_ShaderGUIDs[index];

            Debug.LogError("Trying to access frp shader GUID out of bounds: (" + id + ": " + index + ")");
            return "";
        }
#endif
    }
}