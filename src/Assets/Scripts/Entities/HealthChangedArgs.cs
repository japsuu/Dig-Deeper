namespace Entities
{
    public readonly struct HealthChangedArgs
    {
        public readonly int ChangeAmount;
        public readonly int NewHealth;
        public bool IsDamage => ChangeAmount < 0;
        public bool HasEntityDied => NewHealth <= 0;


        public HealthChangedArgs(int changeAmount, int newHealth)
        {
            ChangeAmount = changeAmount;
            NewHealth = newHealth;
        }
    }
}