using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class HighScoreSettings : MonoBehaviour
    {
        [SerializeField]
        private Button _resetScoreButton;
        
        
        private void Start()
        {
            _resetScoreButton.onClick.AddListener(ResetHighScore);
        }
        
        
        private void ResetHighScore()
        {
            HighScores.ResetHighScores();
        }
    }
}