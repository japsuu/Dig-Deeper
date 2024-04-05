using Audio;
using NaughtyAttributes;
using UnityEngine;
using Utilities;

namespace Weapons.Controllers
{
    /// <summary>
    /// Controls a single weapon and it's rotation.
    /// </summary>
    public class DrillWeaponController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _weaponRotationRoot;
        [SerializeField] private WeaponObject _weapon;
        [SerializeField] private GameObject[] _toggleableObjects;
        [SerializeField] private RotatingObject _alarmLight;

        [Header("Settings")]
        [SerializeField]
        private KeyCode _fireKey = KeyCode.Space;
        
        [SerializeField]
        [Tooltip("How many degrees the weapon can rotate from the center.")]
        [MinMaxSlider(-90, 90)]
        private Vector2 _rotationAngleLimit = new(-45, 45);
        
        [SerializeField]
        [Tooltip("How many degrees the weapon can rotate per second.")]
        private float _rotationSpeed = 10f;

        private float _previousHorizontalAxis;
        private bool _isFiringEnabled;
        
        
        public void SetEnableFiring(bool enable)
        {
            _isFiringEnabled = enable;
            AudioLayer.StopSoundLoop(LoopingSoundType.DRILL_CANNON_ROTATE, transform);
            foreach (GameObject go in _toggleableObjects)
                go.SetActive(enable);
            _alarmLight.SetEnabled(enable);
        }


        private void Awake()
        {
            SetEnableFiring(false);
        }


        private void Update()
        {
            if (!_isFiringEnabled)
                return;
            
            RotateWeapon();

            CheckFiring();
        }


        private void RotateWeapon()
        {
            float axis = Input.GetAxis("Horizontal");
            float rotation = -axis * _rotationSpeed * Time.deltaTime;
            _weaponRotationRoot.Rotate(Vector3.forward, rotation);

            // Clamp the local rotation to the angle limit.
            Vector3 localRotation = _weaponRotationRoot.localEulerAngles;
            localRotation.z = localRotation.z > 180 ? localRotation.z - 360 : localRotation.z;
            localRotation.z = Mathf.Clamp(localRotation.z, _rotationAngleLimit.x, _rotationAngleLimit.y);
            _weaponRotationRoot.localEulerAngles = localRotation;

            if (_previousHorizontalAxis == 0 && axis != 0)
                AudioLayer.PlaySoundLoop(LoopingSoundType.DRILL_CANNON_ROTATE, transform);
            else if (_previousHorizontalAxis != 0 && axis == 0)
                AudioLayer.StopSoundLoop(LoopingSoundType.DRILL_CANNON_ROTATE, transform);
            
            _previousHorizontalAxis = axis;
        }


        private void CheckFiring()
        {
            if (Input.GetKey(_fireKey))
                _weapon.TryFire();
        }


        private void OnDrawGizmos()
        {
            DrawAngleLimitGizmos();
        }


        private void DrawAngleLimitGizmos()
        {
            Vector3 forward = _weaponRotationRoot.right;
            Vector3 left = Quaternion.Euler(0, 0, _rotationAngleLimit.x) * forward;
            Vector3 right = Quaternion.Euler(0, 0, _rotationAngleLimit.y) * forward;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(_weaponRotationRoot.position, left * 10);
            Gizmos.DrawRay(_weaponRotationRoot.position, right * 10);
        }
    }
}