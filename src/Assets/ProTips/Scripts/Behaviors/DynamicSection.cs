﻿using UnityEngine;

namespace ProTips.Scripts.Behaviors
{
    public class DynamicSection : MonoBehaviour
    {
        public string placeholderName;

        /// <summary>The user-friendly name of the placeholder field, without the delimiters.</summary>
        public string Name { get; set; }
    }
}