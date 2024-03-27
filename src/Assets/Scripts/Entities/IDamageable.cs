namespace Entities
{
    public interface IDamageable
    {
        /// <summary>
        /// The team of the damageable entity.
        /// Generally entities belonging to the same team do not damage each other, but it is up to the implementation to decide.
        /// </summary>
        public DamageableTeam Team { get; }
        
        /// <summary>
        /// Whether a projectile should be deleted upon hitting this entity.
        /// </summary>
        public bool DeletesProjectileOnHit { get; }
        
        
        public void Damage(int amount);
    }
}