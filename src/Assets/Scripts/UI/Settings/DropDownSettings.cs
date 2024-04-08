using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UI.Settings
{
    public abstract class DropDownSettings : MonoBehaviour
    {
        [SerializeField]
        private TMP_Dropdown _dropdown;
        
        protected abstract List<string> Options { get; }
        protected abstract int DefaultValue { get; }


        private void Awake()
        {
            // Populate the dropdown with the available Difficulty settings
            _dropdown.ClearOptions();
            _dropdown.AddOptions(Options);
            
            // Set the current quality setting
            _dropdown.value = DefaultValue;
            
            // Refresh the shown value
            _dropdown.RefreshShownValue();
        }


        private void Start()
        {
            _dropdown.onValueChanged.AddListener(OnOptionSelected);
        }


        protected abstract void OnOptionSelected(int index);
    }
}