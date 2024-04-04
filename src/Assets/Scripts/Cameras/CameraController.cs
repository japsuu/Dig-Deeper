using Cinemachine;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cameras
{
    /// <summary>
    /// Keeps track of the current camera and provides access to the virtual camera.
    /// </summary>
    [DefaultExecutionOrder(-1)]
    public class CameraController : SingletonBehaviour<CameraController>
    {
        [SerializeField]
        private string _cameraTargetTag = "CameraTarget";
        
        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;
        
        public Camera MainCamera { get; private set; }
        public CinemachineVirtualCamera VirtualCamera => _virtualCamera;
        public CinemachineBasicMultiChannelPerlin VirtualCameraNoise { get; private set; }
        
        
        public void SetNoiseAmplitude(float amplitude)
        {
            VirtualCameraNoise.m_AmplitudeGain = amplitude;
        }


        private void Awake()
        {
            MainCamera = Camera.main;
            VirtualCameraNoise = _virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            SetNoiseAmplitude(0f);
        }


        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            // Try to find the target object in the scene.
            GameObject player = GameObject.FindGameObjectWithTag(_cameraTargetTag);
            if (player != null)
                _virtualCamera.Follow = player.transform;
            
            SetNoiseAmplitude(0f);
        }
    }
}