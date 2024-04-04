using DamageNumbersPro;
using Entities;
using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Raycast projectile that flies through the air.
    /// Projectiles are a hybrid between simulated rigidbody bullets, and full-on raycasts that travel instantly.
    /// These projectiles do not travel instantly, and have no rigid-bodies.
    /// 
    /// Projectiles raycast forward a certain amount each frame, and travel there if no hit is detected.
    /// If a hit is detected, the projectile teleports to the hit position and triggers a hit.
    /// </summary>
    public class RaycastProjectile : MonoBehaviour
    {
        private const int MAX_RAYCAST_RESULTS = 64;

        [SerializeField]
        private int _weightGrams = 2000;

        [SerializeField]
        private int _baseDamage = 35;

        [SerializeField]
        private bool _debugDraw;

        [SerializeField]
        private LayerMask _projectileHitLayers;

        private bool _awaitingDestruction;
        private RaycastHit2D[] _raycastResults;
        private float _muzzleVelocity;
        private float WeightKilograms => _weightGrams / 1000f;


        public void Initialize(float muzzleVelocity)
        {
            _muzzleVelocity = muzzleVelocity;
            _raycastResults = new RaycastHit2D[MAX_RAYCAST_RESULTS];
        }


        private void FixedUpdate()
        {
            // Safe clause. Might trigger since Destroy(gameObject) does not execute instantly.
            if (_awaitingDestruction)
                return;

            // Calculate how much to move the GameObject along the forward axis (based on muzzleVelocity).
            float distanceToMove = _muzzleVelocity / WeightKilograms * Time.fixedDeltaTime;
            Vector3 debugDrawCachedPos = transform.position;

            // Raycast that distance in front of the bullet before moving: If no collision - move to position, if collision - move to collided point, spawn hit effect, and call DealDamage().
            int size = Physics2D.RaycastNonAlloc(transform.position, transform.right, _raycastResults, distanceToMove, _projectileHitLayers);

            if (size == MAX_RAYCAST_RESULTS)
                Debug.LogWarning($"{nameof(MAX_RAYCAST_RESULTS)} hit, the value probably needs to be raised for hit detection to work correctly!", this);

            for (int i = 0; i < size; i++)
            {
                RaycastHit2D hit = _raycastResults[i];

                transform.position = hit.point;

                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                damageable?.Damage(_baseDamage);

                if (damageable != null)
                {
                    if (!damageable.DeletesProjectileOnHit)
                        continue;
                }
                else
                {
                    if (hit.collider.isTrigger)
                        continue;
                }

                RaycastProjectileRenderer projectileRenderer = GetComponentInChildren<RaycastProjectileRenderer>();

                if (projectileRenderer != null)
                    projectileRenderer.OnProjectileHit(hit, transform.right);

                Destroy(gameObject);
                _awaitingDestruction = true;
                return;
            }

            transform.position += transform.right * distanceToMove;

#if UNITY_EDITOR
            if (_debugDraw)
            {
                Debug.DrawLine(debugDrawCachedPos, transform.position, Color.blue, 2f);
            }
#endif
        }
    }
}