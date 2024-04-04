using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("High Scores")]
        [SerializeField] private TMP_Text _highScoreText;
        
        
        [Header("Play")]
        [SerializeField] private Button _playButton;
        
        
        [Header("Settings")]
        [SerializeField] private Button _openSettingsButton;
        [SerializeField] private Button _closeSettingsButton;
        [SerializeField] private RectTransform _settingsPanel;
        
        
        [Header("Help")]
        [SerializeField] private Button _openHelpButton;
        [SerializeField] private Button _closeHelpButton;
        [SerializeField] private RectTransform _helpPanel;
        
        
        [Header("Credits")]
        [SerializeField] private Button _openCreditsButton;
        [SerializeField] private Button _closeCreditsButton;
        [SerializeField] private RectTransform _creditsPanel;
        
        [Header("Exit")]
        [SerializeField] private Button _exitButton;


        private void Start()
        {
            SetupCursor();
            
            _playButton.onClick.AddListener(OnPlayButtonClicked);
            _exitButton.onClick.AddListener(Application.Quit);
            _openHelpButton.onClick.AddListener(() => _helpPanel.gameObject.SetActive(true));
            _closeHelpButton.onClick.AddListener(() => _helpPanel.gameObject.SetActive(false));
            _openCreditsButton.onClick.AddListener(() => _creditsPanel.gameObject.SetActive(true));
            _closeCreditsButton.onClick.AddListener(() => _creditsPanel.gameObject.SetActive(false));
            _openSettingsButton.onClick.AddListener(() => _settingsPanel.gameObject.SetActive(true));
            _closeSettingsButton.onClick.AddListener(() => _settingsPanel.gameObject.SetActive(false));
            
            _settingsPanel.gameObject.SetActive(false);
            _helpPanel.gameObject.SetActive(false);
            _creditsPanel.gameObject.SetActive(false);

            _highScoreText.text = "Your Highscore: " + HighScores.GetHighScores();
        }


        private static void SetupCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }


        private void OnPlayButtonClicked()
        {
            SceneChanger.LoadGameplayScene();
        }
        
        
        // Hack hack hack because the highscore may be reset, and no time to implement an event
        private const float HIGH_SCORE_REFRESH_RATE = 1f;
        private void Update()
        {
            if (Time.time % HIGH_SCORE_REFRESH_RATE < 0.1f)
                _highScoreText.text = "High Score: " + HighScores.GetHighScores();
        }
    }
}