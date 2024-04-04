using System.Linq;
using TMPro;
using UnityEngine;

namespace UI.Settings
{
    public class DifficultySettings : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown _dropdown;


        private void Awake()
        {
            // Populate the dropdown with the available Difficulty settings
            _dropdown.ClearOptions();
            _dropdown.AddOptions(System.Enum.GetNames(typeof(Difficulty.DifficultyType)).ToList());
            
            // Set the current quality setting
            _dropdown.value = (int)Difficulty.CurrentDifficulty;
            
            // Refresh the shown value
            _dropdown.RefreshShownValue();
        }


        private void Start()
        {
            _dropdown.onValueChanged.AddListener(ChangeDifficulty);
        }
        
        
        private static void ChangeDifficulty(int index)
        {
            Difficulty.CurrentDifficulty = (Difficulty.DifficultyType)index;
        }
    }
}