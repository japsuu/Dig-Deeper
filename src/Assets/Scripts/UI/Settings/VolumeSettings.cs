using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField]
        private Slider _volumeSlider;


        private void Awake()
        {
            UpdateVolume(DefaultVolume.DefaultVolumeValue);
        }


        private void Start()
        {
            _volumeSlider.onValueChanged.AddListener(UpdateVolume);
        }
        
        
        private void UpdateVolume(float volume)
        {
            AudioListener.volume = volume;
        }
    }
}