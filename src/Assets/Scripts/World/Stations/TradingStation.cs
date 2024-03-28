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
        [Range(0.2f, 2f)]
        private float _buyRate = 1f;
        
        
        public void OnDrillEnter()
        {
            Debug.LogWarning($"TODO: Show banner with station name {_name} and buy rate {_buyRate*100}%");
            foreach (Component c in _deletedComponents)
                Destroy(c);
            
            DrillController.Instance.Health.HealToMax();
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
            Debug.LogWarning("TODO: Delete arrow that points to station");
        }


        private void Start()
        {
            Debug.LogWarning("TODO: Create arrow that points to station");
        }
    }
}