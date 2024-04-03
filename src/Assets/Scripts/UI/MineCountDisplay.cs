using System;
using TMPro;
using UnityEngine;
using Weapons.Controllers;

namespace UI
{
    public class MineCountDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;


        private void Awake()
        {
            PlayerMineController.MineCountChanged += OnMineCountChanged;
        }


        private void OnDestroy()
        {
            PlayerMineController.MineCountChanged -= OnMineCountChanged;
        }


        private void OnMineCountChanged(int count)
        {
            _text.text = $"{count}x";
        }
    }
}