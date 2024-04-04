using TMPro;
using UnityEngine;

namespace UI
{
    public class CreditsText : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;

        [SerializeField]
        private string _format = "Credits: {0}";


        private void Awake()
        {
            _text.text = string.Format(_format, 0);

            EventManager.Statistics.CreditsEarnedChanged += OnCreditsEarnedChanged;
        }


        private void OnDestroy()
        {
            EventManager.Statistics.CreditsEarnedChanged -= OnCreditsEarnedChanged;
        }


        private void OnCreditsEarnedChanged(int newCredits)
        {
            _text.text = string.Format(_format, newCredits.ToString("n0"));
        }
    }
}