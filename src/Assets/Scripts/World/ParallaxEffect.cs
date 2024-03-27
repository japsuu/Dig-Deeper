using Cameras;
using UnityEngine;

namespace World
{
    public class ParallaxEffect : MonoBehaviour
    {
        [SerializeField]
        [Range(0f, 1f)]
        private float _parallaxFactor = 0.5f;
        
        private float _startPosition;
        private Transform _cameraTransform;


        private void Start()
        {
            _cameraTransform = CameraController.Instance.MainCamera.transform;
            _startPosition = transform.position.y;
        }


        private void Update()
        {
            float distance = _cameraTransform.position.y * _parallaxFactor;
 
            Vector3 newPosition = new Vector3(transform.position.x, _startPosition + distance, transform.position.z);
 
            transform.position = PixelPerfectClamp(newPosition, Constants.TEXTURE_PPU);
        }
 
        private static Vector3 PixelPerfectClamp(Vector3 locationVector, float pixelsPerUnit)
        {
            Vector3 vectorInPixels = new Vector3(Mathf.CeilToInt(locationVector.x * pixelsPerUnit), Mathf.CeilToInt(locationVector.y * pixelsPerUnit),Mathf.CeilToInt(locationVector.z * pixelsPerUnit));
            return vectorInPixels / pixelsPerUnit;
        }
    }
}