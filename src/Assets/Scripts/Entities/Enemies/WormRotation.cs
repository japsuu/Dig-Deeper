using Audio;
using Entities.Drill;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entities.Enemies
{
    /// <summary>
    /// Selects a random point near the player, and rotates towards that point.
    /// The point is always relative to the player's position, so the worm will travel "along" the player's drill.
    /// </summary>
    public class WormRotation : MonoBehaviour
    {
        [SerializeField]
        private float _rotationSpeed = 90f;
        
        [SerializeField]
        private float _targetMaxDistanceFromPlayer = 15f;

        [SerializeField]
        private bool _playAttackSound = true;
        
        private Vector2 _targetPlayerOffset;
        private Vector2 _targetPosition;    // Target pos in world space. Updated every frame.
        private float _timeSinceReachedTarget;
        
        public bool IsFacingTarget { get; private set; }
        
        
        public void EscapeToSurface()
        {
            // Aim for the surface, away from the player.
            _targetPlayerOffset = new Vector2(0, 200);
            _targetPosition = GetPlayerPosition() + _targetPlayerOffset;
        }

        
        private void Update()
        {
            float targetAngle = CalculateTargetAngle();
            
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, targetAngle), _rotationSpeed * Time.deltaTime);
            
            float angleDifference = Mathf.DeltaAngle(transform.eulerAngles.z, targetAngle);
            IsFacingTarget = Mathf.Abs(angleDifference) < 25;
            
            _timeSinceReachedTarget += Time.deltaTime;
        }


        private float CalculateTargetAngle()
        {
            Vector2 playerPos = GetPlayerPosition();
            
            bool hasReachedTarget = Vector2.Distance(transform.position, _targetPosition) < 1 || _timeSinceReachedTarget > 5f;

            // If close enough to the target position, select a new target offset.
            if (hasReachedTarget)
            {
                _targetPlayerOffset = GetRandomOffset();
                if (_playAttackSound)
                    AudioLayer.PlaySoundOneShot(OneShotSoundType.WORM_ATTACK);
                _timeSinceReachedTarget = 0;
            }
            
            _targetPosition = playerPos + _targetPlayerOffset;
            
            Vector2 direction = (_targetPosition - (Vector2) transform.position).normalized;
            return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        }
        
        
        private Vector2 GetRandomOffset()
        {
            return Random.insideUnitCircle * _targetMaxDistanceFromPlayer;
        }
        
        
        private static Vector2 GetPlayerPosition()
        {
            if (DrillStateMachine.Instance == null)
                return Vector2.zero;
            
            return DrillStateMachine.Instance.transform.position;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_targetPosition, 0.5f);
            Gizmos.DrawLine(transform.position, _targetPosition);
        }
    }
}