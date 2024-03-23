using DG.Tweening;
using UnityEngine;

namespace Drill
{
    /// <summary>
    /// Controls the movement of the drill.
    /// </summary>
    public class DrillMovement : MonoBehaviour
    {
        [SerializeField]
        private float _baseMovementSpeed = 8f;

        [SerializeField]
        private float _movementStartTweenDuration = 2f;

        [SerializeField]
        private float _movementEndTweenDuration = 1f;

        private float _movementSpeed;
        private float _movementSpeedFactor;
        private Rigidbody2D _rigidbody;

        public bool IsEnabled { get; private set; }


        public void Initialize(Rigidbody2D rb)
        {
            _rigidbody = rb;
        }


        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;

            float endSpeed = isEnabled ? _baseMovementSpeed : 0f;
            float duration = isEnabled ? _movementStartTweenDuration : _movementEndTweenDuration;
            DOTween.To(GetMovementSpeed, SetMovementSpeed, endSpeed, duration);
        }
        
        
        public void SetSpeedFactor(float factor)
        {
            _movementSpeedFactor = factor;
        }


        private float GetMovementSpeed() => _movementSpeed;

        private void SetMovementSpeed(float x) => _movementSpeed = x;


        private void FixedUpdate()
        {
            if (IsEnabled)
                _rigidbody.velocity = transform.right * (_movementSpeed * _movementSpeedFactor);
        }
    }
}