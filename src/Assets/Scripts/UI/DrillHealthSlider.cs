using System;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Entities;
using Entities.Drill;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DrillHealthSlider : MonoBehaviour
    {
        [SerializeField]
        private Slider _backgroundSlider;
        
        [SerializeField]
        private Slider _healthSlider;

        private TweenerCore<float, float, FloatOptions> _backgroundTweener;


        private void Start()
        {
            DrillController.Instance.Healed += OnDrillHealed;
            DrillController.Instance.Damaged += OnDrillDamaged;

            _backgroundSlider.maxValue = DrillController.Instance.Health.MaxHealth;
            _healthSlider.maxValue = DrillController.Instance.Health.MaxHealth;
            
            _backgroundSlider.value = DrillController.Instance.Health.CurrentHealth;
            _healthSlider.value = DrillController.Instance.Health.CurrentHealth;
        }


        private void OnDrillHealed(HealthChangedArgs args)
        {
            _backgroundTweener?.Kill();
            _healthSlider.value = args.NewHealth;
            _backgroundSlider.value = args.NewHealth;
        }


        private void OnDrillDamaged(HealthChangedArgs args)
        {
            _backgroundTweener?.Kill();
            _healthSlider.value = args.NewHealth;
            _backgroundTweener = _backgroundSlider.DOValue(args.NewHealth, 0.3f).SetDelay(1f);
        }
    }
}