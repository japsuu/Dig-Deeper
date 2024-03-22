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
        private float _baseRotationSpeed = 90f;
        
        [SerializeField]
        private float _rotationStartTweenDuration = 3f;

        [SerializeField]
        private float _rotationEndTweenDuration = 2f;

        private float _rotationSpeed;

        public bool IsEnabled { get; private set; }
        
        
        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            
            float endSpeed = isEnabled ? _baseRotationSpeed : 0f;
            float duration = isEnabled ? _rotationStartTweenDuration : _rotationEndTweenDuration;
            DOTween.To(GetRotationSpeed, SetRotationSpeed, endSpeed, duration);
        }
        
        
        public void RotateTowardsVelocity(Vector2 velocity, float rotationSpeed)
        {
            float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }


        private float GetRotationSpeed() => _rotationSpeed;
        private void SetRotationSpeed(float x) => _rotationSpeed = x;


        private void Update()
        {
            if (!IsEnabled)
                return;
            
            float rotation = Input.GetAxis("Horizontal") * _rotationSpeed * Time.deltaTime;
            transform.Rotate(Vector3.forward, rotation);

            // Clamp the rotation to prevent the player from going back to the surface.
            float currentAngle = transform.rotation.eulerAngles.z;
            currentAngle = Mathf.Clamp(currentAngle, _rotationAngleLimit.x, _rotationAngleLimit.y);
            transform.rotation = Quaternion.Euler(0f, 0f, currentAngle);
        }
    }
}