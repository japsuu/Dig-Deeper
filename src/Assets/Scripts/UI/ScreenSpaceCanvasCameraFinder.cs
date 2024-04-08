using System;
using Cameras;
using UnityEngine;

namespace UI
{
    public class ScreenSpaceCanvasCameraFinder : MonoBehaviour
    {
        [SerializeField]
        private Canvas _canvas;
        
        
        private void Start()
        {
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            _canvas.worldCamera = CameraController.Instance.MainCamera;
        }
    }
}