using UnityEngine;

namespace UI.Settings
{
    public class DefaultVolume : MonoBehaviour
    {
        [SerializeField]
        private float _defaultVolume = 0.5f;
        
        public static float DefaultVolumeValue { get; private set; }


        private void Awake()
        {
            AudioListener.volume = _defaultVolume;
            DefaultVolumeValue = _defaultVolume;
        }
    }
}