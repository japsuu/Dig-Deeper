using System;
using Entities.Drill;
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


        private void Update()
        {
            CheckKeyboardInput();
        }


        private void CheckKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                _slider.value = 0f;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                _slider.value = 0.5f;
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                _slider.value = 1f;
            
            if (Input.GetKeyDown(KeyCode.W))
                _slider.value = Mathf.Clamp01(_slider.value - 0.1f);
            else if (Input.GetKeyDown(KeyCode.S))
                _slider.value = Mathf.Clamp01(_slider.value + 0.1f);
        }


        private void OnSliderValueChanged(float value)
        {
            float f = Mathf.Lerp(_minSpeed, _maxSpeed, value);
            DrillController.Instance.Movement.SetControlFactor(f);
            DrillController.Instance.Rotation.SetControlFactor(f);
        }
    }
}