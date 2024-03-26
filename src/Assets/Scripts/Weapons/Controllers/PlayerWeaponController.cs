using System;
using UnityEngine;
using World;

namespace Weapons.Controllers
{
    public class PlayerWeaponController : MonoBehaviour
    {
        [Header("References")]
        
        [SerializeField]
        private Transform _weaponRotationRoot;
        
        [SerializeField]
        private WeaponObject _weapon;
        
        [SerializeField]
        private RotatingAlarmLight _alarmLight;

        [Header("Settings")]
        
        [SerializeField]
        private KeyCode _fireKey = KeyCode.Space;
        
        [SerializeField]
        [Tooltip("How many degrees the weapon can rotate to left or right from the center.")]
        private float _rotationLimit = 22f;
        
        [SerializeField]
        [Tooltip("How many degrees the weapon can rotate per second.")]
        private float _rotationSpeed = 10f;

        public bool IsFiringEnabled { get; private set; }
        
        
        public void SetEnableFiring(bool enable)
        {
            IsFiringEnabled = enable;
            _alarmLight.SetEnabled(enable);
        }


        private void Awake()
        {
            SetEnableFiring(false);
        }


        private void Update()
        {
            if (!IsFiringEnabled)
                return;
            
            RotateWeapon();

            CheckFiring();
        }


        private void RotateWeapon()
        {
            float inputX = Input.GetAxis("Horizontal");
            
            float targetRotation = 0f;

            if (inputX < 0f)
                targetRotation = -_rotationLimit;
            else if (inputX > 0f)
                targetRotation = _rotationLimit;

            float rotation = Mathf.MoveTowards(_weaponRotationRoot.localEulerAngles.z, targetRotation, _rotationSpeed * Time.deltaTime);
            
            _weaponRotationRoot.localEulerAngles = new Vector3(0f, 0f, rotation);
        }


        private void CheckFiring()
        {
            if (Input.GetKeyDown(_fireKey))
                _weapon.TryFire();
        }
    }
}