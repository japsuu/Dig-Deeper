using System;
using System.Collections;
using DG.Tweening;
using Singletons;
using UnityEngine;
using World.Chunks;

namespace Drill
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DrillMovement))]
    [RequireComponent(typeof(DrillRotation))]
    public class DrillController : SingletonBehaviour<DrillController>  //TODO: Implement a basic state machine for the drill.
    {
        private const float RB_LINEAR_DRAG_MAX = 15f;
        
        [SerializeField]
        private DrillHead[] _drillHeads;
        
        [SerializeField]
        private TerrainTrigger _terrainTrigger;
        
        [SerializeField]
        private float _rotateTowardsVelocitySpeed = 35f;

        [Header("Hit recovery sequence")]
        
        [SerializeField]
        private float _minVelocityForHitRecovery = 10f;

        private Rigidbody2D _rigidbody;
        private bool _hasDetachedFromDock;
        private bool _isInRecoverySequence;
        private bool _isAirborne;
        
        public DrillMovement Movement { get; private set; }
        public DrillRotation Rotation { get; private set; }
        public DrillInventory Inventory { get; private set; }


        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            Movement = GetComponent<DrillMovement>();
            Rotation = GetComponent<DrillRotation>();
            Inventory = new DrillInventory();

            _rigidbody.simulated = false;
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            
            _terrainTrigger.ContactStart += () => OnHitGround(Mathf.Abs(_rigidbody.velocity.y));
            _terrainTrigger.ContactEnd += OnEnterAirborne;
            
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.Initialize(Inventory);

            Movement.Initialize(_rigidbody);
        }


        private void Start()
        {
            DisableControls();
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
        }


        private void DetachFromDock()
        {
            EnableFalling();
            _rigidbody.simulated = true;
            _hasDetachedFromDock = true;
            _isAirborne = true;
        }


        private void EnableFalling()
        {
            _rigidbody.gravityScale = 1f;
            SetRigidbodyDrag(0f);
            DisableControls();
        }


        private void OnEnterAirborne()
        {
            _isAirborne = true;
            if (_hasDetachedFromDock)
            {
                EnableFalling();
            }
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

            if (hitVelocity < _minVelocityForHitRecovery)
            {
                EnableControls();
                _isInRecoverySequence = false;
                yield break;
            }
            
            yield return new WaitForSeconds(2f);
            
            Debug.LogWarning("TODO: Turn on the lights");
            _rigidbody.velocity = Vector2.zero;
            
            yield return new WaitForSeconds(2f);
            EnableControls();
            _isInRecoverySequence = false;
        }


        private float GetRigidbodyDrag() => _rigidbody.drag;
        private void SetRigidbodyDrag(float x) => _rigidbody.drag = x;
        
        
        private void DisableControls()
        {
            Movement.SetEnabled(false);
            Rotation.SetEnabled(false);
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.SetEnabled(false);
        }


        private void EnableControls()
        {
            SetRigidbodyDrag(0f);
            
            Movement.SetEnabled(true);
            Rotation.SetEnabled(true);
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.SetEnabled(true);
        }
    }
}