using System.Collections.Generic;
using System.Linq;
using TMPro;

namespace UI.Settings
{
    public class QualitySettings : DropDownSettings
    {
        protected override List<string> Options => UnityEngine.QualitySettings.names.ToList();
        protected override int DefaultValue => UnityEngine.QualitySettings.GetQualityLevel();

        
        protected override void OnOptionSelected(int index)
        {
            UnityEngine.QualitySettings.SetQualityLevel(index, true);
        }
    }
}