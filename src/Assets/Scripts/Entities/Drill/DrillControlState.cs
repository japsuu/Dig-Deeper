namespace Entities.Drill
{
    /// <summary>
    /// Determines what part of the drill the player is currently controlling.
    /// </summary>
    public enum DrillControlState
    {
        /// <summary>
        /// The player cannot control any part of the drill.
        /// </summary>
        Nothing,
        
        /// <summary>
        /// The player can detach the drill from the station.
        /// </summary>
        Detach,
        
        /// <summary>
        /// The player can freely move and rotate the drill.
        /// All weapons are disabled.
        /// </summary>
        Movement,
            
        /// <summary>
        /// The player can only use the left weapon.
        /// All movement controls are disabled.
        /// </summary>
        WeaponLeft,
            
        /// <summary>
        /// The player can only use the right weapon.
        /// All movement controls are disabled.
        /// </summary>
        WeaponRight,
    }
}