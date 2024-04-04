using JSAM;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Settings
{
    public class VolumeSettings : MonoBehaviour
    {
        [SerializeField]
        private Slider _masterVolumeSlider;
        
        [SerializeField]
        private Slider _musicVolumeSlider;
        
        [SerializeField]
        private Slider _sfxVolumeSlider;


        private void Awake()
        {
            UpdateVolumeMaster(DefaultVolume.DefaultVolumeValue);
        }


        private void Start()
        {
            _masterVolumeSlider.onValueChanged.AddListener(UpdateVolumeMaster);
            _musicVolumeSlider.onValueChanged.AddListener(UpdateVolumeMusic);
            _sfxVolumeSlider.onValueChanged.AddListener(UpdateVolumeSfx);
            
            _masterVolumeSlider.value = AudioManager.MasterVolume;
            _musicVolumeSlider.value = AudioManager.MusicVolume;
            _sfxVolumeSlider.value = AudioManager.SoundVolume;
        }
        
        
        private static void UpdateVolumeMaster(float volume) => AudioManager.MasterVolume = volume;
        private static void UpdateVolumeMusic(float volume) => AudioManager.MusicVolume = volume;
        private static void UpdateVolumeSfx(float volume) => AudioManager.SoundVolume = volume;
    }
}