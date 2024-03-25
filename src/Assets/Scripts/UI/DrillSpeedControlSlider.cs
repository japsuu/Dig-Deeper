using Drill;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DrillSpeedControlSlider : MonoBehaviour
    {
        [SerializeField]
        private Slider _slider;
        
        [SerializeField]
        private float _minSpeed = 0.2f;
        
        [SerializeField]
        private float _maxSpeed = 1f;
        
        
        private void Start()
        {
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            OnSliderValueChanged(_slider.value);
        }


        private void OnSliderValueChanged(float value)
        {
            float f = Mathf.Lerp(_minSpeed, _maxSpeed, value);
            DrillController.Instance.Movement.SetControlFactor(f);
            DrillController.Instance.Rotation.SetControlFactor(f);
        }
    }
}