namespace Entities.Drill
{
    public class DrillStats
    {
        public ulong TilesMined;
        public ulong CreditsEarned { get; private set; }
        
        
        public void AddCredits(ulong amount)
        {
            CreditsEarned += amount;
            EventManager.OnCreditsEarnedChanged(CreditsEarned);
        }
    }
}