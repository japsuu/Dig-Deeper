using System.Collections;
using DG.Tweening;
using Singletons;
using UnityEngine;

namespace Drill
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(DrillMovement))]
    [RequireComponent(typeof(DrillRotation))]
    public class DrillController : SingletonBehaviour<DrillController>
    {
        [SerializeField]
        private DrillHead[] _drillHeads;
        
        [SerializeField]
        private float _rotateTowardsVelocitySpeed = 35f;

        private Rigidbody2D _rigidbody;
        private DrillMovement _drillMovement;
        private DrillRotation _drillRotation;
        private bool _hasBeenLaunched;
        private bool _hasHitGround;
        
        public DrillInventory Inventory { get; private set; }


        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _drillMovement = GetComponent<DrillMovement>();
            _drillRotation = GetComponent<DrillRotation>();
            Inventory = new DrillInventory();
            
            _rigidbody.simulated = false;
            _rigidbody.bodyType = RigidbodyType2D.Dynamic;
            
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.Initialize(Inventory);
        }


        private void Update()
        {
            if (_hasBeenLaunched)
            {
                if (_hasHitGround)
                    return;
                
                _drillRotation.RotateTowardsVelocity(_rigidbody.velocity, _rotateTowardsVelocitySpeed);
                if (transform.position.y < 0f)
                    StartCoroutine(OnHitGround());
                
                return;
            }

            if (Input.GetKeyUp(KeyCode.Space))
            {
                _rigidbody.simulated = true;
                _hasBeenLaunched = true;
            }
        }
        
        
        private IEnumerator OnHitGround()
        {
            _hasHitGround = true;
            _rigidbody.gravityScale = 0f;
            
            // Start increasing the rb linear drag with a tween to slow down the drill.
            yield return DOTween.To(GetRigidbodyDrag, SetRigidbodyDrag, 15f, 0.7f);

            yield return new WaitForSeconds(2f);
            OnDrillMovementStopped();
            yield return new WaitForSeconds(2f);
            OnDrillStarted();
        }


        private float GetRigidbodyDrag() => _rigidbody.drag;
        private void SetRigidbodyDrag(float x) => _rigidbody.drag = x;


        private void OnDrillMovementStopped()
        {
            Debug.LogWarning("TODO: Turn on the lights");
            _rigidbody.velocity = Vector2.zero;
            _rigidbody.simulated = false;
        }


        private void OnDrillStarted()
        {
            _drillMovement.SetEnabled(true);
            _drillRotation.SetEnabled(true);
            foreach (DrillHead drillHead in _drillHeads)
                drillHead.SetEnabled(true);
        }
    }
}