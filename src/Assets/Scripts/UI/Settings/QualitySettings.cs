using System.Linq;
using TMPro;
using UnityEngine;

namespace UI.Settings
{
    public class QualitySettings : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown _qualityDropdown;


        private void Awake()
        {
            // Populate the dropdown with the available quality settings
            _qualityDropdown.ClearOptions();
            _qualityDropdown.AddOptions(UnityEngine.QualitySettings.names.ToList());
            
            // Set the current quality setting
            _qualityDropdown.value = UnityEngine.QualitySettings.GetQualityLevel();
            
            // Refresh the shown value
            _qualityDropdown.RefreshShownValue();
        }


        private void Start()
        {
            _qualityDropdown.onValueChanged.AddListener(ChangeQuality);
        }
        
        
        private void ChangeQuality(int qualityIndex)
        {
            UnityEngine.QualitySettings.SetQualityLevel(qualityIndex, true);
        }
    }
}