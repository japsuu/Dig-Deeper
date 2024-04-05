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
            GameObject camTarget = GameObject.FindGameObjectWithTag(_cameraTargetTag);
            if (camTarget != null)
                _virtualCamera.Follow = camTarget.transform;
            
            // Try to find a audio listened in the scene. If one is found, replace the listener on the main camera.
            AudioListener mainCamListener = MainCamera.GetComponent<AudioListener>();
            mainCamListener.enabled = true;
            AudioListener[] listeners = FindObjectsOfType<AudioListener>();
            foreach (AudioListener listener in listeners)
            {
                if (listener == mainCamListener)
                    continue;
                
                mainCamListener.enabled = false;
                listener.enabled = true;
                Debug.Log($"Found audio listener on object {listener.gameObject.name}. Replaced the main camera listener.");
            }
            
            SetNoiseAmplitude(0f);
        }
    }
}