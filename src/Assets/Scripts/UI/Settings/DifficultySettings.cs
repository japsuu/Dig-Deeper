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


        public static float GetReceivedDamageMultiplier()
        {
            return CurrentDifficulty switch
            {
                Difficulty.Easy => 0.5f,
                Difficulty.Normal => 1f,
                Difficulty.Mayhem => 1.5f,
                _ => 1f
            };
        }
        
        
        public static float GetChunkPopulationModifier()
        {
            return CurrentDifficulty switch
            {
                Difficulty.Easy => 0.5f,
                Difficulty.Normal => 1f,
                Difficulty.Mayhem => 3f,
                _ => 1f
            };
        }
    }
}