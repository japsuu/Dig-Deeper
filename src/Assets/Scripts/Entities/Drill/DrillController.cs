using System;
using System.Collections;
using Audio;
using Cinemachine;
using DG.Tweening;
using Singletons;
using UnityEngine;
using Weapons.Controllers;
using World.Chunks;

namespace Entities.Drill
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DrillMovement))]
    [RequireComponent(typeof(DrillRotation))]
    public class DrillController : SingletonBehaviour<DrillController>
    {
        private const float RB_LINEAR_DRAG_MAX = 15f;

        public event Action<HealthChangedArgs> Healed;
        public event Action<HealthChangedArgs> Damaged;
        public event Action Killed;

        
        [Header("References")]
        [SerializeField] private EntityHealth _health;
        [SerializeField] private TerrainTrigger _terrainTrigger;
        [SerializeField] private Transform _lightsRoot;
        [SerializeField] private CinemachineImpulseSource _collisionImpulseSource;
        
        
        [Header("Weapons")]
        [SerializeField] private PlayerWeaponController _leftWeapon;
        [SerializeField] private PlayerWeaponController _rightWeapon;
        [SerializeField] private KeyCode _leftWeaponActivationKey = KeyCode.Q;
        [SerializeField] private KeyCode _rightWeaponActivationKey = KeyCode.E;

        
        [Header("Collision")]
        [SerializeField] private float _rotateTowardsVelocitySpeed = 35f;
        [SerializeField] private float _minVelocityForHitRecovery = 14f;
        
        private DrillHead[] _drillHeads;
        private Rigidbody2D _rigidbody;
        
        private DrillState _state;                  // In what state the drill currently is.
        private DrillControlState _controlState;    // What part of the drill the player is currently controlling.
        
        public DrillMovement Movement { get; private set; }
        public DrillRotation Rotation { get; private set; }
        public DrillInventory Inventory { get; private set; }
        public EntityHealth Health => _health;


        private void Awake()
        {
            _drillHeads = GetComponentsInChildren<DrillHead>();
            _rigidbody = GetComponent<Rigidbody2D>();
            Movement = GetComponent<DrillMovement>();
            Rotation = GetComponent<DrillRotation>();
            Inventory = new DrillInventory();

            _terrainTrigger.ContactStart += () => ChangeDrillState(DrillState.Crashed);
            _terrainTrigger.ContactEnd += () => ChangeDrillState(DrillState.Airborne);
            
            _health.HealthChanged += OnHealthChanged;
            _health.Killed += () => ChangeDrillState(DrillState.Destroyed);
            
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.Initialize(Inventory);

            Movement.Initialize(_rigidbody);
            _controlState = DrillControlState.Movement;
        }


        private void Start()
        {
            ChangeDrillState(DrillState.Docked, true);
        }


        private void Update()
        {
            UpdateDrillState();
        }


        #region DRILL STATE MACHINE

        private void ChangeDrillState(DrillState state, bool force = false)
        {
            // Skip state checks and exit callbacks if forced, since the exit callback can potentially change the state.
            if (!force)
            {
                if (_state == state)
                    return;
                
                OnDrillExitState(_state);
            }
            
            _state = state;
            OnDrillEnterState(_state);
        }
        
        
        private void OnDrillEnterState(DrillState state)
        {
            switch (state)
            {
                case DrillState.Docked:
                {
                    _rigidbody.simulated = false;
                    
                    ChangeControlState(DrillControlState.Movement);
                    break;
                }
                case DrillState.Airborne:
                {
                    _rigidbody.gravityScale = 1f;
                    SetRigidbodyDrag(0f);
                    break;
                }
                case DrillState.Crashed:
                {
                    float hitVelocity = Mathf.Abs(_rigidbody.velocity.y);
                    StartCoroutine(CrashSequence(hitVelocity));
                    break;
                }
                case DrillState.Controlled:
                {
                    Rotation.SetEnabled(true);
                    Movement.SetEnabled(true);
                    SetDrillsEnabled(true);
                    break;
                }
                case DrillState.Destroyed:
                {
                    Rotation.SetEnabled(false);
                    Movement.SetEnabled(false);
                    SetDrillsEnabled(false);
                    SetLightsEnabled(false);
                    _leftWeapon.SetEnableFiring(false);
                    _rightWeapon.SetEnableFiring(false);
            
                    Killed?.Invoke();
            
                    Debug.LogWarning("TODO: Implement drill destruction.");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
        }
        
        
        private void UpdateDrillState()
        {
            switch (_state)
            {
                case DrillState.Docked:
                {
                    if (Input.GetKeyUp(KeyCode.Space))
                        ChangeDrillState(DrillState.Airborne);
                    break;
                }
                case DrillState.Airborne:
                {
                    Rotation.RotateTowardsVelocity(_rigidbody.velocity, _rotateTowardsVelocitySpeed);
                    break;
                }
                case DrillState.Crashed:
                {
                    break;
                }
                case DrillState.Controlled:
                {
                    UpdateControlState();
                    break;
                }
                case DrillState.Destroyed:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        
        private void OnDrillExitState(DrillState state)
        {
            switch (state)
            {
                case DrillState.Docked:
                {
                    _rigidbody.simulated = true;
                    break;
                }
                case DrillState.Airborne:
                {
                    _rigidbody.gravityScale = 0f;
                    break;
                }
                case DrillState.Crashed:
                {
                    break;
                }
                case DrillState.Controlled:
                {
                    Rotation.SetEnabled(false);
                    Movement.SetEnabled(false);
                    SetDrillsEnabled(false);
                    break;
                }
                case DrillState.Destroyed:
                {
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
                }
            }
        }

        #endregion


        #region CONTROL STATE MACHINE

        private void ChangeControlState(DrillControlState controlState)
        {
            if (_controlState == controlState)
                return;
            
            OnControlExitState(_controlState);
            _controlState = controlState;
            OnControlEnterState(_controlState);
        }
        
        
        private void OnControlEnterState(DrillControlState controlState)
        {
            switch (controlState)
            {
                case DrillControlState.Movement:
                {
                    Rotation.SetEnabled(true);
                    break;
                }
                case DrillControlState.WeaponLeft:
                {
                    _leftWeapon.SetEnableFiring(true);
                    break;
                }
                case DrillControlState.WeaponRight:
                {
                    _rightWeapon.SetEnableFiring(true);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(controlState), controlState, null);
                }
            }
        }
        
        
        private void UpdateControlState()
        {
            switch (_controlState)
            {
                case DrillControlState.Movement:
                {
                    Rotation.RotateTowardsInput();
                    
                    if (Input.GetKeyUp(_leftWeaponActivationKey))
                        ChangeControlState(DrillControlState.WeaponLeft);
                    else if (Input.GetKeyUp(_rightWeaponActivationKey))
                        ChangeControlState(DrillControlState.WeaponRight);
                    
                    break;
                }
                case DrillControlState.WeaponLeft:
                {
                    if (Input.GetKeyUp(_leftWeaponActivationKey))
                        ChangeControlState(DrillControlState.Movement);
                    else if (Input.GetKeyUp(_rightWeaponActivationKey))
                        ChangeControlState(DrillControlState.WeaponRight);

                    break;
                }
                case DrillControlState.WeaponRight:
                {
                    if (Input.GetKeyUp(_leftWeaponActivationKey))
                        ChangeControlState(DrillControlState.WeaponLeft);
                    else if (Input.GetKeyUp(_rightWeaponActivationKey))
                        ChangeControlState(DrillControlState.Movement);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }


        private void OnControlExitState(DrillControlState controlState)
        {
            switch (controlState)
            {
                case DrillControlState.Movement:
                {
                    Rotation.SetEnabled(false);
                    break;
                }
                case DrillControlState.WeaponLeft:
                {
                    _leftWeapon.SetEnableFiring(false);
                    break;
                }
                case DrillControlState.WeaponRight:
                {
                    _rightWeapon.SetEnableFiring(false);
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(controlState), controlState, null);
                }
            }
        }

        #endregion


        private void OnHealthChanged(HealthChangedArgs healthChangedArgs)
        {
            if (healthChangedArgs.IsDamage)
                Damaged?.Invoke(healthChangedArgs);
            else
                Healed?.Invoke(healthChangedArgs);
        }


        private void SetDrillsEnabled(bool enable)
        {
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.SetEnabled(enable);
        }
        
        
        private void SetLightsEnabled(bool enable)
        {
            _lightsRoot.gameObject.SetActive(enable);
        }
        
        
        private IEnumerator CrashSequence(float hitVelocity)
        {
            if (hitVelocity >= _minVelocityForHitRecovery)
            {
                _collisionImpulseSource.GenerateImpulse();
                AudioManager.PlaySound("thump large");
            }
            
            // Start increasing the rb linear drag with a tween to slow down the drill.
            // NOTE: We could also tween the velocity to zero, but that could cause issues if we were to fall through a very thin surface.
            const float duration = 0.7f;
            yield return DOTween.To(GetRigidbodyDrag, SetRigidbodyDrag, RB_LINEAR_DRAG_MAX, duration).OnComplete(() =>
            {
                SetRigidbodyDrag(0f);
            });
            
            if (hitVelocity >= _minVelocityForHitRecovery)
                yield return HitRecoverySequence();
            
            ChangeDrillState(DrillState.Controlled);
        }


        private IEnumerator HitRecoverySequence()
        {
            AudioManager.PlaySound("crash warning");
            yield return new WaitForSeconds(0.2f);
            
            SetDrillsEnabled(false);
            SetLightsEnabled(false);
            
            yield return new WaitForSeconds(1.5f);
            AudioManager.PlaySound("rebooting");
            
            SetDrillsEnabled(true);
            SetLightsEnabled(true);
            
            _rigidbody.velocity = Vector2.zero;
            
            yield return new WaitForSeconds(0.8f);
        }


        private float GetRigidbodyDrag() => _rigidbody.drag;
        private void SetRigidbodyDrag(float x) => _rigidbody.drag = x;
    }
}