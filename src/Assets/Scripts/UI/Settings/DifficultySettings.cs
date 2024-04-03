using System.Linq;
using TMPro;
using UnityEngine;

namespace UI.Settings
{
    public class DifficultySettings : MonoBehaviour
    {
        public static Difficulty CurrentDifficulty = Difficulty.Normal;
        
        public enum Difficulty
        {
            Easy,
            Normal,
            Mayhem
        }
        
        [SerializeField]
        private TMP_Dropdown _dropdown;


        private void Awake()
        {
            // Populate the dropdown with the available Difficulty settings
            _dropdown.ClearOptions();
            _dropdown.AddOptions(System.Enum.GetNames(typeof(Difficulty)).ToList());
            
            // Set the current quality setting
            _dropdown.value = (int)CurrentDifficulty;
            
            // Refresh the shown value
            _dropdown.RefreshShownValue();
        }


        private void Start()
        {
            _dropdown.onValueChanged.AddListener(ChangeDifficulty);
        }
        
        
        private void ChangeDifficulty(int index)
        {
            CurrentDifficulty = (Difficulty)index;
        }


        public static float GetLevelPower()
        {
            return CurrentDifficulty switch
            {
                Difficulty.Easy => 1f,
                Difficulty.Normal => 1.5f,
                Difficulty.Mayhem => 3f,
                _ => 1.5f
            };
        }


        public static int GetInitialEnemyCount()
        {
            return CurrentDifficulty switch
            {
                Difficulty.Easy => 1,
                Difficulty.Normal => 2,
                Difficulty.Mayhem => 5,
                _ => 2
            };
        }
    }
}