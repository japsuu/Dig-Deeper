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
            if (_terrainDigger != null)
                _terrainDigger.SetUpdateMode(ScriptUpdateMode.Manual);

            if (TradingStation.IsPlayerInStation)
            {
                DestroyRecursive();
                return;
            }
            
            EventManager.TradingStations.PlayerEnterStation += OnTradingStationEnter;
        }


        private void Start()
        {
            AudioLayer.PlaySoundOneShot(OneShotSoundType.WORM_SPAWN, transform);
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
            if (DrillStateMachine.Instance != null && Vector2.Distance(transform.position, DrillStateMachine.Instance.transform.position) > _despawnDistance)
            {
                DestroyRecursive();
                return;
            }
            
            base.Update();

            if (_rotation.IsFacingTarget && _terrainDigger != null)
                _terrainDigger.ManualUpdate();
        }


        public override void Damage(int amount)
        {
            AudioLayer.PlaySoundOneShot(OneShotSoundType.WORM_HIT_CRITICAL, transform);
            base.Damage(amount);
        }


        protected override void OnKilled()
        {
            // If the head is killed, the whole worm should be destroyed
            DestroyRecursive();
            
            AudioLayer.PlaySoundOneShot(OneShotSoundType.WORM_DEATH, transform);
        }
    }
}