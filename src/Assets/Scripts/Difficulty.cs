/// <summary>
/// Provides access to the current difficulty of the game.
/// </summary>
public static class Difficulty
{
    public static DifficultyType CurrentDifficulty = DifficultyType.Normal;

    public enum DifficultyType
    {
        Easy,
        Normal,
        Mayhem
    }


    public static float GetReceivedDamageMultiplier()
    {
        return CurrentDifficulty switch
        {
            DifficultyType.Easy => 0.5f,
            DifficultyType.Normal => 1f,
            DifficultyType.Mayhem => 1.5f,
            _ => 1f
        };
    }


    public static float GetChunkPopulationModifier()
    {
        return CurrentDifficulty switch
        {
            DifficultyType.Easy => 0.5f,
            DifficultyType.Normal => 1f,
            DifficultyType.Mayhem => 3f,
            _ => 1f
        };
    }
}