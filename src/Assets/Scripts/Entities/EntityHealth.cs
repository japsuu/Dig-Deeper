using System;
using NaughtyAttributes;
using UnityEngine;
using World;

namespace Entities
{
    /// <summary>
    /// Component that handles the health of an entity.
    /// Used on objects where composition is preferred over inheritance (<see cref="DamageableEntity"/>).
    /// </summary>
    public sealed class EntityHealth : MonoBehaviour, IDamageable
    {
        /// <summary>
        /// Called when the health of the entity changes.
        /// </summary>
        public event Action<HealthChangedArgs> HealthChanged;
        
        /// <summary>
        /// Called when the entity is killed.
        /// </summary>
        public event Action Killed;
        
        [SerializeField] private DamageableTeam _team = DamageableTeam.Neutral;
        [SerializeField] private bool _deleteProjectileOnHit = true;
        [SerializeField] private int _maxHealth = 100;
        [SerializeField] private float _receivedDamageMultiplier = 1f;
        
        public int CurrentHealth { get; private set; }
        public int MaxHealth => _maxHealth;
        public DamageableTeam Team => _team;
        public bool DeletesProjectileOnHit => _deleteProjectileOnHit;
        public float ReceivedDamageMultiplier => _receivedDamageMultiplier;
        public void SetReceivedDamageMultiplier(float receivedDamageMultiplier) => _receivedDamageMultiplier = receivedDamageMultiplier;
        
        
        public void SetMaxHealth(int maxHealth, bool healToMax = false)
        {
            _maxHealth = maxHealth;
            
            if (healToMax)
                CurrentHealth = maxHealth;
        }
        
        
        public void Heal(int amount)
        {
            if (amount <= 0 || CurrentHealth >= _maxHealth)
                return;
            
            CurrentHealth = Mathf.Min(CurrentHealth + amount, _maxHealth);
            HealthChanged?.Invoke(new HealthChangedArgs(amount, CurrentHealth));
        }


        public void Damage(int amount)
        {
            if (amount <= 0 || CurrentHealth <= 0)
                return;
            
            int damage = Mathf.CeilToInt(amount * _receivedDamageMultiplier);
            CurrentHealth -= damage;
            
            DamageNumberSystem.Instance.SpawnDamageNumber(transform.position, damage, true);

            HealthChanged?.Invoke(new HealthChangedArgs(-amount, CurrentHealth));
            
            if (CurrentHealth > 0)
                return;
            
            Kill();
        }
        
        
        [Button("Kill")]
        public void Kill()
        {
            CurrentHealth = 0;
            Killed?.Invoke();
        }
        
        [Button("Heal to max")]
        public void HealToMax() => Heal(_maxHealth);


        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }
    }
}