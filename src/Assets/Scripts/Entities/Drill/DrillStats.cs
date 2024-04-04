namespace Entities.Drill
{
    /// <summary>
    /// Statistics of the current player drill.
    /// </summary>
    public class DrillStats
    {
        public int TilesMined;
        public int CreditsEarned { get; private set; }
        
        
        public void AddCredits(int amount)
        {
            CreditsEarned += amount;
            EventManager.Statistics.OnCreditsEarnedChanged(CreditsEarned);
        }
    }
}