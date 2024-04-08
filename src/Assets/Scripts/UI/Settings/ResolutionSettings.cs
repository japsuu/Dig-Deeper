using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.Settings
{
    public class ResolutionSettings : DropDownSettings
    {
        protected override List<string> Options => Screen.resolutions.Select(resolution => $"{resolution.width}x{resolution.height}").ToList();
        protected override int DefaultValue => Screen.resolutions.ToList().FindIndex(resolution => resolution.width == Screen.currentResolution.width && resolution.height == Screen.currentResolution.height);

        
        protected override void OnOptionSelected(int index)
        {
            Resolution resolution = Screen.resolutions[index];
            Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        }
    }
}