using Audio;
using Entities.Drill;
using UnityEngine;
using World.Chunks;
using World.Stations;

namespace Entities.Enemies
{
    [RequireComponent(typeof(WormRotation))]
    public class WormHead : WormPart
    {
        [SerializeField]
        private TerrainDigger _terrainDigger;
        
        [SerializeField]
        private float _despawnDistance = 80f;

        private WormRotation _rotation;
        private Vector2 _previousPosition;


        protected override void Awake()
        {
            base.Awake();
            
            _rotation = GetComponent<WormRotation>();
            _terrainDigger._scriptUpdateMode = ScriptUpdateMode.Manual;

            if (TradingStation.IsPlayerInStation)
            {
                DestroyRecursive();
                return;
            }
            
            EventManager.TradingStations.PlayerEnterStation += OnTradingStationEnter;
        }


        private void OnDestroy()
        {
            EventManager.TradingStations.PlayerEnterStation -= OnTradingStationEnter;
        }


        private void OnTradingStationEnter(TradingStation s)
        {
            // Worms are afraid of trading stations, so turn away.
            _rotation.EscapeToSurface();
            Invoke(nameof(DestroyRecursive), 5f);
        }


        protected override void Update()
        {
            if (Vector2.Distance(transform.position, DrillController.Instance.transform.position) > _despawnDistance)
            {
                DestroyRecursive();
                return;
            }
            
            base.Update();

            if (_rotation.IsFacingTarget)
                _terrainDigger.ManualUpdate();
        }


        public override void Damage(int amount)
        {
            AudioManager.PlaySound("worm hit critical");
            base.Damage(amount);
        }


        protected override void OnKilled()
        {
            // If the head is killed, the whole worm should be destroyed
            DestroyRecursive();
        }
    }
}