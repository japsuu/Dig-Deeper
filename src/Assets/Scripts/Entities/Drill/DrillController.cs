using System;
using System.Collections;
using Audio;
using Cameras;
using Cinemachine;
using DG.Tweening;
using NaughtyAttributes;
using Singletons;
using UI.Settings;
using UnityEngine;
using UnityEngine.Events;
using Weapons.Controllers;
using World.Chunks;
using World.Stations;

namespace Entities.Drill
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DrillMovement))]
    [RequireComponent(typeof(DrillRotation))]
    [RequireComponent(typeof(DrillMineController))]
    public class DrillController : SingletonBehaviour<DrillController>
    {
        private const float RB_LINEAR_DRAG_MAX = 15f;
        
        [SerializeField] private UnityEvent _onGameStart;
        [SerializeField] private UnityEvent _onGameStop;

        
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

        
        [Header("Debug")]
        [SerializeField] private bool _enableVerboseLogging;
        [SerializeField, ReadOnly] private DrillState _state;                  // In what state the drill currently is.
        [SerializeField, ReadOnly] private DrillControlState _controlState;    // What part of the drill the player is currently controlling.
        
        private DrillHead[] _drillHeads;
        private Rigidbody2D _rigidbody;
        private TradingStation _currentTradingStation;
        private Coroutine _activeCrashSequence;
        private bool _isFirstImpact = true;
        
        public EntityHealth Health => _health;
        public DrillStats Stats { get; private set; }
        public DrillMovement Movement { get; private set; }
        public DrillRotation Rotation { get; private set; }
        public DrillInventory Inventory { get; private set; }
        public DrillMineController MineController { get; private set; }


        private void Awake()
        {
            _drillHeads = GetComponentsInChildren<DrillHead>();
            _rigidbody = GetComponent<Rigidbody2D>();
            Movement = GetComponent<DrillMovement>();
            Rotation = GetComponent<DrillRotation>();
            MineController = GetComponent<DrillMineController>();
            Inventory = new DrillInventory();
            Stats = new DrillStats();

            _terrainTrigger.ContactStart += OnTerrainContactStart;
            _terrainTrigger.ContactEnd += OnTerrainContactEnd;
            
            _health.HealthChanged += OnHealthChanged;
            _health.Killed += () => ChangeDrillState(DrillState.Destroyed);
            _health.SetReceivedDamageMultiplier(DifficultySettings.GetReceivedDamageMultiplier());
            
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.Initialize(Inventory, Stats);

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
            if (_activeCrashSequence != null)
            {
                StopCoroutine(_activeCrashSequence);
                _activeCrashSequence = null;
                LogVerbose("Force stopped active crash sequence.");
            }
            
            // Skip state checks and exit callbacks if forced, since the exit callback can potentially change the state.
            if (!force)
            {
                if (_state == state)
                    return;
                
                OnDrillExitState(_state);
            }
            
            _state = state;
            LogVerbose($"DrillState changed to {_state}");
            OnDrillEnterState(_state);
        }
        
        
        private void OnDrillEnterState(DrillState state)
        {
            switch (state)
            {
                case DrillState.Docked:
                {
                    if (_currentTradingStation != null)
                        _currentTradingStation.OnDrillEnter();
                    _rigidbody.simulated = false;
                    _rigidbody.gravityScale = 0f;
                    _rigidbody.velocity = Vector2.zero;
                    
                    ChangeControlState(DrillControlState.Movement);
                    break;
                }
                case DrillState.Airborne:
                {
                    _rigidbody.gravityScale = 1f;
                    SetRigidbodyDrag(0f);
                    AudioLayer.PlaySoundLoop(LoopingSoundType.DRILL_FALLING);
                    break;
                }
                case DrillState.Crashed:
                {
                    if (_isFirstImpact)
                    {
                        EventManager.PlayerDrill.OnDrillFirstImpact();
                        _isFirstImpact = false;
                    }
                    float hitVelocity = Mathf.Abs(_rigidbody.velocity.y);
                    _activeCrashSequence = StartCoroutine(CrashSequence(hitVelocity));
                    break;
                }
                case DrillState.Controlled:
                {
                    Rotation.SetEnabled(true);
                    Movement.SetEnabled(true);
                    SetDrillsEnabled(true);
                    AudioLayer.PlaySoundLoop(LoopingSoundType.DRILL_DRILLING);
                    CameraController.Instance.SetNoiseAmplitude(1f);
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
            
                    EventManager.PlayerDrill.OnDrillKilled();
                    AudioLayer.PlaySoundOneShot(OneShotSoundType.DRILL_EXPLOSION);
            
                    HighScores.SaveHighScore(Stats.CreditsEarned);
                    
                    _onGameStop.Invoke();
            
                    SceneChanger.LoadMainMenuScene();
                    break;
                }
                case DrillState.StationInbound:
                {
                    _rigidbody.gravityScale = 1f;
                    SetRigidbodyDrag(0f);
                    break;
                }
                case DrillState.StationOutbound:
                {
                    _rigidbody.gravityScale = 1f;
                    SetRigidbodyDrag(0f);
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
                    if (Input.GetKeyUp(KeyCode.F))
                    {
                        DrillState newState = _currentTradingStation == null ? DrillState.Airborne : DrillState.StationOutbound;
                        ChangeDrillState(newState);
                    }
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
                case DrillState.StationInbound:
                {
                    if (transform.position.y <= _currentTradingStation.transform.position.y)
                    {
                        _currentTradingStation.TeleportDrillToStation();
                        ChangeDrillState(DrillState.Docked);
                    }
                    break;
                }
                case DrillState.StationOutbound:
                {
                    Rotation.RotateTowardsVelocity(_rigidbody.velocity, _rotateTowardsVelocitySpeed);
                    if (_currentTradingStation.HasDrillExited())
                    {
                        DrillState nextState = _terrainTrigger.IsTriggered ? DrillState.Crashed : DrillState.Airborne;
                        ChangeDrillState(nextState);
                    }
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
                    if (_currentTradingStation != null)
                        _currentTradingStation.OnDrillExit();
                    _rigidbody.simulated = true;
                    
                    if(_isFirstImpact)
                        _onGameStart.Invoke();
                    break;
                }
                case DrillState.Airborne:
                {
                    _rigidbody.gravityScale = 0f;
                    AudioLayer.StopSoundLoop(LoopingSoundType.DRILL_FALLING);
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
                    AudioLayer.StopSoundLoop(LoopingSoundType.DRILL_DRILLING);
                    CameraController.Instance.SetNoiseAmplitude(0f);
                    break;
                }
                case DrillState.Destroyed:
                {
                    break;
                }
                case DrillState.StationInbound:
                {
                    _rigidbody.gravityScale = 0f;
                    break;
                }
                case DrillState.StationOutbound:
                {
                    _currentTradingStation = null;
                    _rigidbody.gravityScale = 0f;
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

                    if (Input.GetKey(KeyCode.Space))
                        MineController.TryLayMine();
                    
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


        private void OnTerrainContactStart()
        {
            if (_state != DrillState.Airborne)
                return;
            
            ChangeDrillState(DrillState.Crashed);
        }


        private void OnTerrainContactEnd()
        {
            if (_activeCrashSequence != null)
            {
                StopCoroutine(_activeCrashSequence);
                _activeCrashSequence = null;
                LogVerbose("Terrain contact lost, force stopped active crash sequence.");
            }

            // Ignore terrain contacts when moving to a station.
            if (_state != DrillState.Controlled)
                return;
            
            ChangeDrillState(DrillState.Airborne);
        }


        private void OnHealthChanged(HealthChangedArgs healthChangedArgs)
        {
            if (healthChangedArgs.IsDamage)
                EventManager.PlayerDrill.OnDrillDamaged(healthChangedArgs);
            else
                EventManager.PlayerDrill.OnDrillHealed(healthChangedArgs);
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
                AudioLayer.PlaySoundOneShot(OneShotSoundType.THUMP_LARGE);
            }
            else
            {
                AudioLayer.PlaySoundOneShot(OneShotSoundType.THUMP_SMALL);
            }
            
            LogVerbose($"Drill crashed with velocity: {hitVelocity}");
            
            // Start increasing the rb linear drag with a tween to slow down the drill.
            // NOTE: We could also tween the velocity to zero, but that could cause issues if we were to fall through a very thin surface.
            const float duration = 0.7f;
            yield return DOTween.To(GetRigidbodyDrag, SetRigidbodyDrag, RB_LINEAR_DRAG_MAX, duration).OnComplete(() =>
            {
                SetRigidbodyDrag(0f);
            });
            
            if (hitVelocity >= _minVelocityForHitRecovery)
            {
                _activeCrashSequence = StartCoroutine(HitRecoverySequence());
                yield return _activeCrashSequence;
                LogVerbose($"Exit {nameof(HitRecoverySequence)}");
            }
            
            _activeCrashSequence = null;
            ChangeDrillState(DrillState.Controlled);
        }


        private IEnumerator HitRecoverySequence()
        {
            LogVerbose($"Enter {nameof(HitRecoverySequence)}");
            AudioLayer.PlaySoundOneShot(OneShotSoundType.DRILL_CRASH_WARNING);
            yield return new WaitForSeconds(0.2f);
            
            SetDrillsEnabled(false);
            SetLightsEnabled(false);
            
            yield return new WaitForSeconds(1.5f);
            AudioLayer.PlaySoundOneShot(OneShotSoundType.DRILL_REBOOTING);

            SetDrillsEnabled(true);
            SetLightsEnabled(true);
            
            _rigidbody.velocity = Vector2.zero;
            
            yield return new WaitForSeconds(0.8f);
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Station"))
                return;
            
            TradingStation station = other.GetComponent<TradingStation>();
            
            OnEnterStation(station);
        }


        private float GetRigidbodyDrag() => _rigidbody.drag;
        private void SetRigidbodyDrag(float x) => _rigidbody.drag = x;


        private void OnEnterStation(TradingStation tradingStation)
        {
            _currentTradingStation = tradingStation;
            
            // Set the drill to free-fall, and stop when reached the station y-level.
            ChangeDrillState(DrillState.StationInbound);
        }
        
        
        private void LogVerbose(string message)
        {
            if (_enableVerboseLogging)
                Debug.Log(message);
        }
    }
}