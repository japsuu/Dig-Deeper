using System;
using Entities;
using World.Stations;

/// <summary>
/// Centralized event manager for static events.
/// </summary>
public static class EventManager
{
    /// <summary>
    /// Contains statics related events.
    /// </summary>
    public static class Statistics
    {
        public static event Action<ulong> TilesMinedChanged;
        public static event Action<ulong> CreditsEarnedChanged;

        public static void OnTilesMinedChanged(ulong tilesMined) => TilesMinedChanged?.Invoke(tilesMined);
        public static void OnCreditsEarnedChanged(ulong creditsEarned) => CreditsEarnedChanged?.Invoke(creditsEarned);
    }

    /// <summary>
    /// Contains player drill related events.
    /// </summary>
    public static class PlayerDrill
    {
        public static event Action<HealthChangedArgs> DrillHealed;
        public static event Action<HealthChangedArgs> DrillDamaged;
        public static event Action DrillKilled;
        public static event Action DrillFirstImpact;

        public static void OnDrillHealed(HealthChangedArgs args) => DrillHealed?.Invoke(args);
        public static void OnDrillDamaged(HealthChangedArgs args) => DrillDamaged?.Invoke(args);
        public static void OnDrillKilled() => DrillKilled?.Invoke();
        public static void OnDrillFirstImpact() => DrillFirstImpact?.Invoke();
    }

    /// <summary>
    /// Contains player inventory related events.
    /// </summary>
    public static class PlayerInventory
    {
        public static event Action<byte, uint> MaterialCountChanged;
        
        public static void OnMaterialCountChanged(byte material, uint count) => MaterialCountChanged?.Invoke(material, count);
    }

    /// <summary>
    /// Contains trading station related events.
    /// </summary>
    public static class TradingStations
    {
        public static event Action StationCreated;
        public static event Action StationDeleted;
        public static event Action<TradingStation> PlayerEnterStation;
        public static event Action<TradingStation> PlayerExitStation;

        public static void OnStationCreated() => StationCreated?.Invoke();
        public static void OnStationDeleted() => StationDeleted?.Invoke();
        public static void OnPlayerEnterStation(TradingStation station) => PlayerEnterStation?.Invoke(station);
        public static void OnPlayerExitStation(TradingStation station) => PlayerExitStation?.Invoke(station);
    }
}