using UnityEngine;

/// <summary>
/// Keeps track of the player's high scores, and saves/loads them through PlayerPrefs.
/// </summary>
public static class HighScores
{
    private const string CREDITS_EARNED_KEY = "CreditsEarned";
    private const string DEPTH_KEY = "DepthReached";
        
    
    public static void SaveHighScores(int creditsEarned, int depth)
    {
        (int highestCredits, int highestDepth) = GetHighScores();
        
        if (creditsEarned > highestCredits)
            PlayerPrefs.SetInt(CREDITS_EARNED_KEY, creditsEarned);
        
        if (depth > highestDepth)
            PlayerPrefs.SetInt(DEPTH_KEY, depth);
    }
        
        
    public static (int credits, int depth) GetHighScores()
    {
        return (PlayerPrefs.GetInt(CREDITS_EARNED_KEY, 0), PlayerPrefs.GetInt(DEPTH_KEY, 0));
    }


    public static void ResetHighScores()
    {
        PlayerPrefs.SetInt(CREDITS_EARNED_KEY, 0);
        PlayerPrefs.SetInt(DEPTH_KEY, 0);
    }
}