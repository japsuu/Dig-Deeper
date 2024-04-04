using System;

namespace UI.Typewriter
{
    [Serializable]
    public class TypeWriterPauseInfo
    {
        public float DotPause = 1f;
        public float CommaPause = 0.5f;
        public float SpacePause = 0.3f;
        public float NormalPause = 0.1f;
    }
}