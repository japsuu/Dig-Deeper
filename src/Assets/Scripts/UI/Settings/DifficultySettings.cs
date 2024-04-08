using System;
using System.Collections.Generic;
using System.Linq;

namespace UI.Settings
{
    public class DifficultySettings : DropDownSettings
    {
        protected override List<string> Options => Enum.GetNames(typeof(Difficulty.DifficultyType)).ToList();
        protected override int DefaultValue => (int)Difficulty.CurrentDifficulty;

        
        protected override void OnOptionSelected(int index)
        {
            Difficulty.CurrentDifficulty = (Difficulty.DifficultyType)index;
        }
    }
}