namespace Entities
{
    public interface IDamageable
    {
        /// <summary>
        /// The team of the damageable entity.
        /// Generally entities belonging to the same team do not damage each other, but it is up to the implementation to decide.
        /// </summary>
        public DamageableTeam Team { get; }
        
        
        public void Damage(int amount);
    }
}