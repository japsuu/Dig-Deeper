using Cinemachine;
using Singletons;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cameras
{
    public class CameraController : SingletonBehaviour<CameraController>
    {
        [SerializeField]
        private string _cameraTargetTag = "CameraTarget";
        
        [SerializeField]
        private CinemachineVirtualCamera _virtualCamera;
        
        public Camera MainCamera { get; private set; }


        private void Awake()
        {
            MainCamera = Camera.main;
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private void OnSceneLoaded(Scene s, LoadSceneMode m)
        {
            // Try to find the target object in the scene.
            GameObject player = GameObject.FindGameObjectWithTag(_cameraTargetTag);
            if (player != null)
                _virtualCamera.Follow = player.transform;
        }
    }
}