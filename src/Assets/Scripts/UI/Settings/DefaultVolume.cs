using JSAM;
using UnityEngine;

namespace UI.Settings
{
    /// <summary>
    /// Sets the volume to a default value on program start.
    /// </summary>
    public class DefaultVolume : MonoBehaviour
    {
        [SerializeField] private float _defaultVolume = 0.5f;
        
        public static float DefaultVolumeValue { get; private set; }


        private void Start()
        {
            AudioManager.MasterVolume = _defaultVolume;
            DefaultVolumeValue = _defaultVolume;
        }
    }
}