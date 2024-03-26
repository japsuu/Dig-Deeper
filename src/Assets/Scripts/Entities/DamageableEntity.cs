using NaughtyAttributes;
using UnityEngine;

namespace Entities
{
    /// <summary>
    /// Base-class for damageable entities.
    /// </summary>
    public abstract class DamageableEntity : MonoBehaviour, IDamageable
    {
        [SerializeField]
        private DamageableTeam _team = DamageableTeam.Neutral;
        
        [SerializeField]
        private int _maxHealth = 100;
        
        [SerializeField]
        private float _receivedDamageMultiplier = 1f;
        
        private int _currentHealth;
        
        public int MaxHealth => _maxHealth;
        public float ReceivedDamageMultiplier => _receivedDamageMultiplier;
        public DamageableTeam Team => _team;


        protected virtual void Awake()
        {
            _currentHealth = _maxHealth;
        }
        
        
        public void SetMaxHealth(int maxHealth, bool healToMax = false)
        {
            _maxHealth = maxHealth;
            
            if (healToMax)
                _currentHealth = maxHealth;
        }
        
        
        public void SetReceivedDamageMultiplier(float receivedDamageMultiplier)
        {
            _receivedDamageMultiplier = receivedDamageMultiplier;
        }


        public virtual void Damage(int amount)
        {
            int damage = Mathf.CeilToInt(amount * _receivedDamageMultiplier);
            _currentHealth -= damage;
            
            if (_currentHealth > 0)
                return;
            
            Kill();
        }
        
        
        [Button("Kill")]
        public void Kill()
        {
            _currentHealth = 0;
            OnKilled();
        }
        
        
        protected abstract void OnKilled();
    }
}