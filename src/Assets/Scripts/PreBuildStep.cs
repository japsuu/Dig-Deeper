﻿#if UNITY_EDITOR && UNITY_WEBGL && WEBGL_THREADING_TEST

using UnityEditor;
[InitializeOnLoad]
public class PreBuildStep 
{
    static PreBuildStep()
    {
        PlayerSettings.WebGL.linkerTarget = WebGLLinkerTarget.Wasm;
        PlayerSettings.WebGL.emscriptenArgs = "-s ALLOW_MEMORY_GROWTH=1";
        PlayerSettings.WebGL.threadsSupport = true;
        PlayerSettings.WebGL.memorySize = 2032;
    }
}

#endif