﻿using Audio;
using Effects;
using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Fires <see cref="RaycastProjectile"/>s.
    /// </summary>
    public class WeaponObject : MonoBehaviour
    {
        [Header("References")]

        [SerializeField]
        private Transform _firePoint;

        [SerializeField]
        private RaycastProjectile _projectilePrefab;

        [SerializeField]
        private OneShotParticleSystem _muzzleFlashPrefab;

        [Header("Settings")]
        
        [SerializeField]
        [Tooltip("The rounds per minute of the weapon.")]
        private float _fireRateRpm = 120f;

        [SerializeField]
        [Range(10f, 1000f)]
        private float _muzzleVelocity = 250f;
        
        private float _fireDelayLeft;


        public void TryFire()
        {
            if (_fireDelayLeft > 0f)
                return;

            Fire();
            _fireDelayLeft = 60f / _fireRateRpm;
        }


        private void Fire()
        {
            // Generate a projectile.
            RaycastProjectile projectile = Instantiate(_projectilePrefab, _firePoint.position, _firePoint.rotation);
            projectile.Initialize(_muzzleVelocity);

            // Generate a muzzle flash.
            if (_muzzleFlashPrefab != null)
                Instantiate(_muzzleFlashPrefab, _firePoint.position, _firePoint.rotation);
            
            AudioManager.PlaySound("fire");
        }


        private void Update()
        {
            UpdateFiringDelay();
        }


        private void UpdateFiringDelay()
        {
            if (_fireDelayLeft > 0f)
                _fireDelayLeft -= Time.deltaTime;
        }
    }
}