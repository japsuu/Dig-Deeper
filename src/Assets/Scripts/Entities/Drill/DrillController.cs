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
    public class DrillController : SingletonBehaviour<DrillController>  //TODO: Implement a basic state machine for the drill.
    {
        private const float RB_LINEAR_DRAG_MAX = 15f;

        public event Action<HealthChangedArgs> Healed;
        public event Action<HealthChangedArgs> Damaged;
        public event Action Killed;

        [Header("References")]
        
        [SerializeField]
        private EntityHealth _health;
        
        [SerializeField]
        private TerrainTrigger _terrainTrigger;
        
        [SerializeField]
        private Transform _lightsRoot;
        
        [SerializeField]
        private Transform _particlesRoot;
        
        [SerializeField]
        private CinemachineImpulseSource _collisionImpulseSource;
        
        [Header("Weapons")]

        [SerializeField]
        private PlayerWeaponController _leftWeapon;
        
        [SerializeField]
        private PlayerWeaponController _rightWeapon;

        [SerializeField]
        private KeyCode _leftWeaponActivationKey = KeyCode.Q;
        
        [SerializeField]
        private KeyCode _rightWeaponActivationKey = KeyCode.E;

        [Header("Collision")]
        
        [SerializeField]
        private float _rotateTowardsVelocitySpeed = 35f;
        
        [SerializeField]
        private float _minVelocityForHitRecovery = 14f;
        
        private DrillHead[] _drillHeads;
        private Rigidbody2D _rigidbody;
        private bool _hasDetachedFromDock;
        private bool _isInRecoverySequence;
        private bool _isAirborne;
        
        public DrillMovement Movement { get; private set; }
        public DrillRotation Rotation { get; private set; }
        public DrillInventory Inventory { get; private set; }
        public EntityHealth Health => _health;


        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            Movement = GetComponent<DrillMovement>();
            Rotation = GetComponent<DrillRotation>();
            _drillHeads = GetComponentsInChildren<DrillHead>();
            Inventory = new DrillInventory();

            _rigidbody.simulated = false;
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            
            _terrainTrigger.ContactStart += () => OnHitGround(Mathf.Abs(_rigidbody.velocity.y));
            _terrainTrigger.ContactEnd += OnEnterAirborne;
            
            _health.HealthChanged += OnHealthChanged;
            _health.Killed += OnKilled;
            
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.Initialize(Inventory);

            Movement.Initialize(_rigidbody);
        }


        private void Start()
        {
            SetMovementControlsEnabled(false);
        }


        private void Update()
        {
            if (!_hasDetachedFromDock)
            {
                if (Input.GetKeyUp(KeyCode.Space))
                    DetachFromDock();
                return;
            }
            
            if (_isInRecoverySequence)
                return;
            
            if (_isAirborne)
                Rotation.RotateTowardsVelocity(_rigidbody.velocity, _rotateTowardsVelocitySpeed);

            if (Input.GetKeyUp(_leftWeaponActivationKey))
            {
                bool isFiringEnabled = _leftWeapon.IsFiringEnabled;
                Rotation.SetEnabled(isFiringEnabled);
                _leftWeapon.SetEnableFiring(!isFiringEnabled);

                if (!isFiringEnabled && _rightWeapon.IsFiringEnabled)
                    _rightWeapon.SetEnableFiring(false);
            }
            
            if (Input.GetKeyUp(_rightWeaponActivationKey))
            {
                bool isFiringEnabled = _rightWeapon.IsFiringEnabled;
                Rotation.SetEnabled(isFiringEnabled);
                _rightWeapon.SetEnableFiring(!isFiringEnabled);

                if (!isFiringEnabled && _leftWeapon.IsFiringEnabled)
                    _leftWeapon.SetEnableFiring(false);
            }
        }


        private void OnHealthChanged(HealthChangedArgs healthChangedArgs)
        {
            if (healthChangedArgs.IsDamage)
                Damaged?.Invoke(healthChangedArgs);
            else
                Healed?.Invoke(healthChangedArgs);
        }


        private void OnKilled()
        {
            SetDrillEnabled(false);
            SetMovementControlsEnabled(false);
            SetLightsEnabled(false);
            _leftWeapon.SetEnableFiring(false);
            _rightWeapon.SetEnableFiring(false);
            
            Killed?.Invoke();
            
            Debug.LogWarning("TODO: Implement drill destruction.");
        }


        private void DetachFromDock()
        {
            EnableFalling();
            _rigidbody.simulated = true;
            _hasDetachedFromDock = true;
            _isAirborne = true;
        }


        private void OnEnterAirborne()
        {
            _isAirborne = true;
            if (_hasDetachedFromDock)
            {
                EnableFalling();
            }
        }


        private void EnableFalling()
        {
            _rigidbody.gravityScale = 1f;
            SetRigidbodyDrag(0f);
            SetMovementControlsEnabled(false);
        }


        private void OnHitGround(float hitVelocity)
        {
            StartCoroutine(HitRecoverySequence(hitVelocity));
        }
        
        
        private IEnumerator HitRecoverySequence(float hitVelocity)
        {
            _isAirborne = false;
            _isInRecoverySequence = true;
            _rigidbody.gravityScale = 0f;
            
            // Start increasing the rb linear drag with a tween to slow down the drill.
            yield return DOTween.To(GetRigidbodyDrag, SetRigidbodyDrag, RB_LINEAR_DRAG_MAX, 0.7f).OnComplete(() => SetRigidbodyDrag(0f));
            
            if (hitVelocity >= _minVelocityForHitRecovery)
            {
                _collisionImpulseSource.GenerateImpulse();
                AudioManager.PlaySound("crash warning");
                yield return new WaitForSeconds(0.2f);
            
                SetLightsEnabled(false);
                yield return new WaitForSeconds(1.5f);
                SetLightsEnabled(true);
                
                AudioManager.PlaySound("rebooting");
                _rigidbody.velocity = Vector2.zero;
            
                yield return new WaitForSeconds(0.8f);
            }
            
            SetMovementControlsEnabled(true);
            SetDrillEnabled(true);
            
            _isInRecoverySequence = false;
        }
        
        
        private void SetMovementControlsEnabled(bool enable)
        {
            Movement.SetEnabled(enable);
            Rotation.SetEnabled(enable);
        }


        private void SetDrillEnabled(bool enable)
        {
            if (enable)
            {
                foreach (DrillHead drillHead in _drillHeads)
                    drillHead.SetEnabled(true);
                _particlesRoot.gameObject.SetActive(true);
            }
            else
            {
                foreach (DrillHead drillHead in _drillHeads)
                    drillHead.SetEnabled(false);
                _particlesRoot.gameObject.SetActive(false);
            }
        }
        
        
        private void SetLightsEnabled(bool enable)
        {
            _lightsRoot.gameObject.SetActive(enable);
        }


        private float GetRigidbodyDrag() => _rigidbody.drag;
        private void SetRigidbodyDrag(float x) => _rigidbody.drag = x;
    }
}