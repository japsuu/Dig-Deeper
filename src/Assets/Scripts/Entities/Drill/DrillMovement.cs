using DG.Tweening;
using UnityEngine;
using World.Chunks;

namespace Entities.Drill
{
    /// <summary>
    /// Controls the movement of the drill.
    /// </summary>
    public class DrillMovement : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private Transform _hardnessCheckTransform;

        [Header("Speed")]
        [SerializeField]
        [Tooltip("The speed moved at when terrain hardness is at minimum.")]
        private float _minMovementSpeed = 2f;

        [SerializeField]
        [Tooltip("The speed moved at when terrain hardness is at maximum.")]
        private float _maxMovementSpeed = 8f;
        
        
        [Header("Tweening")]
        [SerializeField] private float _movementStartTweenDuration = 2f;
        [SerializeField] private float _movementEndTweenDuration = 1f;

        private float _movementSpeed;
        private float _controlFactor;
        private float _tweenFactor;
        private float _terrainHardnessFactor;
        private Rigidbody2D _rigidbody;

        public bool IsEnabled { get; private set; }


        public void Initialize(Rigidbody2D rb)
        {
            _rigidbody = rb;
        }


        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;

            float endF = isEnabled ? 1f : 0f;
            float duration = isEnabled ? _movementStartTweenDuration : _movementEndTweenDuration;
            DOTween.To(GetTweenFactor, SetTweenFactor, endF, duration);
        }
        
        
        public void SetControlFactor(float factor)
        {
            _controlFactor = factor;
        }


        private void Update()
        {
            CalculateTerrainHardness();
            
            _movementSpeed = Mathf.Lerp(_minMovementSpeed, _maxMovementSpeed, _terrainHardnessFactor) * _controlFactor * _tweenFactor;
        }


        private void CalculateTerrainHardness()
        {
            byte hardness = ChunkManager.Instance.GetTerrainHardnessAt(_hardnessCheckTransform.position);
            
            float factor = 1f - hardness / 255f;
            _terrainHardnessFactor = Mathf.Lerp(_terrainHardnessFactor, factor, Time.deltaTime);
            
            DrillStateMachine.Instance.Rotation.SetTerrainHardnessFactor(_terrainHardnessFactor);
        }


        private void FixedUpdate()
        {
            if (IsEnabled)
                _rigidbody.velocity = transform.right * _movementSpeed;
        }


        private float GetTweenFactor() => _tweenFactor;
        private void SetTweenFactor(float x) => _tweenFactor = x;
    }
}