using System;

public static class EventManager
{
    public static event Action<ulong> TilesMinedChanged;
    public static event Action<ulong> CreditsEarnedChanged;
    
    
    public static void OnTilesMinedChanged(ulong tilesMined) => TilesMinedChanged?.Invoke(tilesMined);
    public static void OnCreditsEarnedChanged(ulong creditsEarned) => CreditsEarnedChanged?.Invoke(creditsEarned);
}