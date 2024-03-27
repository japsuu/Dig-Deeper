namespace Entities
{
    /// <summary>
    /// Used to determine if certain entities can damage each other.
    /// </summary>
    public enum DamageableTeam
    {
        /// <summary>
        /// May damage and be damaged by any entity.
        /// </summary>
        Neutral,
        
        /// <summary>
        ///May damage and be damaged by any entity except for other players.
        /// </summary>
        Player,
        
        /// <summary>
        /// May damage and be damaged by any entity except for other enemies.
        /// </summary>
        Enemy,
    }
}