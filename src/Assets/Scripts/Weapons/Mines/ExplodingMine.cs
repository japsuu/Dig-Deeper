using Entities;
using UnityEngine;

namespace Weapons.Mines
{
    public class ExplodingMine : DamageableEntity
    {
        [SerializeField] private GameObject _explosionEffect;

        [SerializeField] private float _explosionRadius = 5f;
        [SerializeField] private float _explosionDamage = 60f;

        [SerializeField]
        [Tooltip("How long after being laid the mine will explode.")]
        private float _explodeAfterSeconds = 5f;
        
        [SerializeField] private AnimationCurve _explosionDamageFalloff;
        [SerializeField] private LayerMask _explodeOnCollisionLayers;

        private readonly Collider2D[] _nearbyColliders = new Collider2D[8];
        private float _lifetime;


        private void Start()
        {
            _lifetime = _explodeAfterSeconds;
        }


        private void Update()
        {
            _lifetime -= Time.deltaTime;
            
            if (_lifetime <= 0f)
                OnKilled();
        }


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_explodeOnCollisionLayers != (_explodeOnCollisionLayers | (1 << other.gameObject.layer)))
                return;

            OnKilled();
        }


        protected override void OnKilled()
        {
            Instantiate(_explosionEffect, transform.position, Quaternion.identity);

            int count = Physics2D.OverlapCircleNonAlloc(transform.position, _explosionRadius, _nearbyColliders);
            for (int i = 0; i < count; i++)
            {
                GameObject go = _nearbyColliders[i].gameObject;
                
                // Skip self.
                if(go == gameObject)
                    continue;

                if (!go.TryGetComponent(out IDamageable damageable))
                    continue;

                // Determine where shrapnel hit the damageable.
                Vector2 hitPosition = _nearbyColliders[i].ClosestPoint(transform.position);

                // Calculate damage based on distance and damage falloff.
                float distance = Vector2.Distance(hitPosition, transform.position);
                float damageFactor = _explosionDamageFalloff.Evaluate(distance / _explosionRadius);
                int damageAmount = Mathf.RoundToInt(_explosionDamage * damageFactor);
                damageable.Damage(damageAmount);
            }
            
            Destroy(gameObject);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            
            Gizmos.DrawWireSphere(transform.position, _explosionRadius);
        }
    }
}