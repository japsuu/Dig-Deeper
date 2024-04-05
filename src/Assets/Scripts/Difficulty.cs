using UnityEngine;

/// <summary>
/// Provides access to the current difficulty of the game.
/// </summary>
public static class Difficulty
{
    private const float CHUNK_MIN_POPULATION_MODIFIER = 0.3f;
    private const float CHUNK_MAX_POPULATION_MODIFIER = 3f;
    private const float CHUNK_MAX_POPULATION_MODIFIER_DEPTH = 3000;
    
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


    public static float GetChunkPopulationModifier(float chunkDepth)
    {
        float difficultyModifier = CurrentDifficulty switch
        {
            DifficultyType.Easy => 0.5f,
            DifficultyType.Normal => 1f,
            DifficultyType.Mayhem => 3f,
            _ => 1f
        };
        
        float depthProgress = Mathf.Clamp(chunkDepth / CHUNK_MAX_POPULATION_MODIFIER_DEPTH, 0, 1);
        float depthModifier = Mathf.Lerp(CHUNK_MIN_POPULATION_MODIFIER, CHUNK_MAX_POPULATION_MODIFIER, depthProgress);
        
        return depthModifier * difficultyModifier;
    }
}