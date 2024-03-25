using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;

namespace Drill
{
    /// <summary>
    /// Controls the rotation of the drill.
    /// </summary>
    public class DrillRotation : MonoBehaviour
    {
        [SerializeField]
        [MinMaxSlider(181, 359)]
        private Vector2 _rotationAngleLimit = new(225f, 315f);

        [SerializeField]
        private float _minRotationSpeed = 20f;
        
        [SerializeField]
        private float _maxRotationSpeed = 90f;
        
        [SerializeField]
        private float _rotationStartTweenDuration = 3f;

        [SerializeField]
        private float _rotationEndTweenDuration = 2f;

        
        private float _rotationSpeed;
        private float _controlFactor;
        private float _tweenFactor;
        private float _terrainHardnessFactor;

        public bool IsEnabled { get; private set; }
        
        
        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            
            float endF = isEnabled ? 1f : 0f;
            float duration = isEnabled ? _rotationStartTweenDuration : _rotationEndTweenDuration;
            DOTween.To(GetTweenFactor, SetTweenFactor, endF, duration);
        }
        
        
        public void RotateTowardsVelocity(Vector2 velocity, float rotationSpeed)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        
        public void SetControlFactor(float factor)
        {
            _controlFactor = factor;
        }
        
        
        public void SetTerrainHardnessFactor(float factor)
        {
            _terrainHardnessFactor = factor;
        }


        private void Update()
        {
            if (!IsEnabled)
                return;
            
            _rotationSpeed = Mathf.Lerp(_minRotationSpeed, _maxRotationSpeed, _terrainHardnessFactor) * _controlFactor * _tweenFactor;
            
            float rotation = Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, rotation);

            // Clamp the rotation to prevent the player from going back to the surface.
            float currentAngle = transform.rotation.eulerAngles.z;
            currentAngle = Mathf.Clamp(currentAngle, _rotationAngleLimit.x, _rotationAngleLimit.y);
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }


        private float GetTweenFactor() => _tweenFactor;
        private void SetTweenFactor(float x) => _tweenFactor = x;
    }
}