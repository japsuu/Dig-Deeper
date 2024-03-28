using System;
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
        public static event Action<TradingStation> PlayerEnter;
        public static event Action<TradingStation> PlayerExit;
        
        [SerializeField]
        [Tooltip("The position to teleport the drill to when the player enters the station.")]
        private Transform _drillTargetPosition;
        
        [SerializeField]
        [Tooltip("The position below which the player is considered to have left the station area.")]
        private Transform _leaveHeightReference;

        [SerializeField]
        [Tooltip("Components to destroy when the player has entered the station.")]
        private Component[] _deletedComponents;
        
        [SerializeField]
        [Tooltip("The name of this station. Used for the UI popup.")]
        private string _name = "Trading Station";
        
        [SerializeField]
        [TextArea]
        [Tooltip("The description of this station. Used for the UI popup.")]
        private string _description = "It's been a while since I've seen anyone around these parts! I'm always looking for new materials to buy, and happen to have a lot of credits to spare. I'll buy anything you've got, at a fair price!";
        
        [SerializeField]
        [Range(0.2f, 2f)]
        private float _buyRate = 1f;

        public string Name => _name;
        public string DescriptionText => GetDescriptionText();
        public string InfoText => GetInfoText();
        public float BuyRate => _buyRate;


        public void OnDrillEnter()
        {
            Debug.LogWarning($"TODO: Show banner with station name {Name} and buy rate {_buyRate*100}%");
            foreach (Component c in _deletedComponents)
                Destroy(c);
            
            DrillController.Instance.Health.HealToMax();
            PlayerEnter?.Invoke(this);
        }
        
        
        public void TeleportDrillToStation()
        {
            DrillController.Instance.transform.SetPositionAndRotation(_drillTargetPosition.position, _drillTargetPosition.rotation);
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
            PlayerExit?.Invoke(this);
        }
        
        
        public void SellPlayerMaterials()
        {
            int sellValue = CalculateSellValue();
            if (sellValue <= 0)
                return;
            DrillController.Instance.Inventory.Clear();
            DrillController.Instance.Stats.CreditsEarned += (ulong)sellValue;
        }


        private int CalculateSellValue()
        {
            return Mathf.RoundToInt(DrillController.Instance.Inventory.GetTotalValue() * _buyRate);
        }


        private string GetDescriptionText() =>
            @$"
Trading station says:
<i>{_description}</i>
";


        private string GetInfoText() =>
            @$"
Your drill has been fixed, and ammunition restocked.

Buy rate: {_buyRate*100}%
Press <b>[E]</b> to sell all materials for <b>{CalculateSellValue()}</b> credits.

Press <b>[F]</b> to leave.
";
    }
}