﻿using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using Entities;
using Entities.Drill;
using UnityEngine;
using UnityEngine.UI;

namespace UI.HUD
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
            EventManager.PlayerDrill.DrillHealed += OnDrillHealed;
            EventManager.PlayerDrill.DrillDamaged += OnDrillDamaged;

            _backgroundSlider.maxValue = DrillStateMachine.Instance.Health.MaxHealth;
            _healthSlider.maxValue = DrillStateMachine.Instance.Health.MaxHealth;
            
            _backgroundSlider.value = DrillStateMachine.Instance.Health.CurrentHealth;
            _healthSlider.value = DrillStateMachine.Instance.Health.CurrentHealth;
        }


        private void OnDestroy()
        {
            EventManager.PlayerDrill.DrillHealed -= OnDrillHealed;
            EventManager.PlayerDrill.DrillDamaged -= OnDrillDamaged;
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