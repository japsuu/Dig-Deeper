using System;
using Audio;
using Entities.Drill;
using UnityEngine;

namespace World.Stations
{
    /// <summary>
    /// Allows the player to sell their gathered materials for points.
    /// Heals the player and fills refills their ammunition.
    /// Different trading stations may have different buying rates, to encourage high risk high reward gameplay.
    /// </summary>
    public class TradingStation : MonoBehaviour
    {
        public static bool IsPlayerInStation { get; private set; }
        
        [SerializeField]
        [Tooltip("The position to teleport the drill to when the player enters the station.")]
        private Transform _drillTargetPosition;
        
        [SerializeField]
        [Tooltip("The position below which the player is considered to have left the station area.")]
        private Transform _leaveHeightReference;

        [SerializeField]
        [Tooltip("Components to destroy when the player has entered the station.")]
        private Component[] _deletedComponents;
        
        [Header("UI")]
        
        [SerializeField]
        [Tooltip("The name of this station. Used for the UI popup.")]
        private string _name = "Trading Station";
        
        [SerializeField]
        [TextArea]
        [Tooltip("The description title of this station. Used for the UI popup.")]
        private string _descriptionTitle = "Trading station says:\n<i>{0}</i>";
        
        [SerializeField]
        [TextArea]
        [Tooltip("The description of this station. Used for the UI popup.")]
        private string _description = "It's been a while since I've seen anyone around these parts! I'm always looking for new materials to buy, and happen to have a lot of credits to spare. I'll buy anything you've got, at a fair price!";
        
        [SerializeField]
        [TextArea]
        [Tooltip("The info of this station. Used for the UI popup.")]
        private string _info = "Your drill has been fixed, and ammunition restocked.\n\nBuy rate: {0}%\nPress <b>[E]</b> to sell all materials for <b>{1}</b> credits.\n\nPress <b>[F]</b> to leave.";
        
        [SerializeField]
        [Range(0.2f, 2f)]
        private float _buyRate = 1f;

        public string Name => _name;


        public void OnDrillEnter()
        {
            foreach (Component c in _deletedComponents)
                Destroy(c);
            
            DrillController.Instance.Health.HealToMax();
            IsPlayerInStation = true;
            EventManager.TradingStations.OnPlayerEnterStation(this);
        }
        
        
        public void TeleportDrillToStation()
        {
            DrillController.Instance.transform.SetPositionAndRotation(_drillTargetPosition.position, _drillTargetPosition.rotation);
            AudioLayer.PlaySoundOneShot(OneShotSoundType.STATION_ENTER);
        }
        
        
        public bool HasDrillExited()
        {
            return DrillController.Instance.transform.position.y < _leaveHeightReference.position.y;
        }


        public void OnDrillExit()
        {
            foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
                c.enabled = false;
            Debug.LogWarning("TODO: Show banner with 'Goodbye!'");
            IsPlayerInStation = false;
            EventManager.TradingStations.OnPlayerExitStation(this);
        }
        
        
        public void SellPlayerMaterials()
        {
            int sellValue = CalculateSellValue();
            if (sellValue <= 0)
                return;
            DrillController.Instance.Inventory.Clear();
            DrillController.Instance.Stats.AddCredits((ulong)sellValue);
            AudioLayer.PlaySoundOneShot(OneShotSoundType.STATION_SELL_MATERIALS);
            AudioLayer.PlaySoundOneShot(OneShotSoundType.STATION_RECEIVE_CREDITS);
        }


        private int CalculateSellValue()
        {
            return Mathf.RoundToInt(DrillController.Instance.Inventory.GetTotalValue() * _buyRate);
        }


        public string GetDescriptionText() => string.Format(_descriptionTitle, _description);
        public string GetInfoText() => string.Format(_info, _buyRate * 100, CalculateSellValue());
    }
}