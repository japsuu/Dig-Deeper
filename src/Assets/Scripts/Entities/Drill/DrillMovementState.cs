namespace Entities.Drill
{
    /// <summary>
    /// Determines the movement state of the drill.
    /// May affect the current <see cref="DrillControlState"/>.
    /// </summary>
    public enum DrillMovementState
    {
        /// <summary>
        /// The drill is docked at an station.
        /// The player can only detach the drill from the station.
        /// This may be the starting station or a generated station.
        /// </summary>
        Docked,
        
        /// <summary>
        /// The drill is falling, and cannot be controlled by the player.
        /// </summary>
        Airborne,
        
        /// <summary>
        /// The drill has crashed into the ground, and cannot be controlled by the player.
        /// May or may not trigger the recovery sequence.
        /// </summary>
        Crashed,
        
        /// <summary>
        /// The drill is currently in the recovery sequence, and cannot be controlled by the player.
        /// </summary>
        CrashRecovery,
        
        /// <summary>
        /// The drill is currently being controlled by the player.
        /// </summary>
        Controlled,
    }
}