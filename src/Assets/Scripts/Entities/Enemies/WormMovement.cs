using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace Entities.Enemies
{
    /// <summary>
    /// Moves a worm forward and changes its speed based on its state.
    /// </summary>
    [RequireComponent(typeof(WormRotation))]
    public class WormMovement : MonoBehaviour
    {
        private enum State
        {
            Roaming,
            Chasing
        }

        [SerializeField]
        [Tooltip("The speed the worm normally moves at.")]
        private float _roamingMovementSpeed = 10f;

        [SerializeField]
        [Tooltip("The speed the worm moves at when chasing the player.")]
        private float _chasingMovementSpeed = 15f;

        [SerializeField]
        private float _movementSpeedChangeDuration = 2f;

        private float _movementSpeed;
        private State _currentState;
        private WormRotation _rotation;
        private TweenerCore<float, float, FloatOptions> _speedTweener;

        private float GetSpeed() => _movementSpeed;

        private void SetSpeed(float x) => _movementSpeed = x;


        private void Awake()
        {
            _rotation = GetComponent<WormRotation>();
        }


        private void Start()
        {
            OnStateEnter(_currentState);
        }


        private void TweenMovementSpeed(float to)
        {
            _speedTweener?.Kill();
            _speedTweener = DOTween.To(GetSpeed, SetSpeed, to, _movementSpeedChangeDuration);
        }


        private void Update()
        {
            OnStateUpdate(_currentState);

            transform.position += transform.right * (_movementSpeed * Time.deltaTime);
        }


        private void ChangeState(State newState)
        {
            if (_currentState == newState)
                return;

            OnStateExit(_currentState);
            _currentState = newState;
            OnStateEnter(newState);
        }


        private void OnStateEnter(State state)
        {
            switch (state)
            {
                case State.Roaming:
                    TweenMovementSpeed(_roamingMovementSpeed);
                    break;
                case State.Chasing:
                    TweenMovementSpeed(_chasingMovementSpeed);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }


        private void OnStateExit(State state)
        {
            switch (state)
            {
                case State.Roaming:
                    break;
                case State.Chasing:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }


        private void OnStateUpdate(State state)
        {
            switch (state)
            {
                case State.Roaming:
                    if (_rotation.IsFacingTarget)
                        ChangeState(State.Chasing);
                    break;
                case State.Chasing:
                    if (!_rotation.IsFacingTarget)
                        ChangeState(State.Roaming);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}