using Entities;
using UnityEngine;

namespace World
{
    public class HealthPickup : MonoBehaviour
    {
        [SerializeField] private int _healAmount = 500;
        [SerializeField] private LayerMask _collisionMask;
        
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collisionMask != (_collisionMask | (1 << other.gameObject.layer)))
                return;

            if (!other.TryGetComponent(out EntityHealth health))
                return;
            
            health.Heal(_healAmount);
            Destroy(gameObject);
        }
    }
}