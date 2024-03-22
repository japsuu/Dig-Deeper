using Singletons;
using UnityEngine;

namespace Cameras
{
    public class CameraController : SingletonBehaviour<CameraController>
    {
        public Camera MainCamera { get; private set; }


        private void Awake()
        {
            MainCamera = Camera.main;
        }
    }
}