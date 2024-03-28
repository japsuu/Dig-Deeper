using UnityEngine;
using World.Chunks;

namespace Entities.Drill
{
    public class DrillHead : TerrainDigger
    {
        private const int BREAK_INTERVAL_FRAMES_CUTSCENE = 2;
        private const int BREAK_INTERVAL_FRAMES = 8;
        
        [SerializeField]
        private Transform _particlesRoot;

        private DrillInventory _inventory;
        private DrillStats _stats;

        public bool IsEnabled { get; private set; }


        private void Awake()
        {
            BreakIntervalFrames = BREAK_INTERVAL_FRAMES_CUTSCENE;
        }


        public void Initialize(DrillInventory inventory, DrillStats stats)
        {
            _inventory = inventory;
            _stats = stats;
        }
        
        
        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            BreakIntervalFrames = isEnabled ? BREAK_INTERVAL_FRAMES : BREAK_INTERVAL_FRAMES_CUTSCENE;
            
            if (_particlesRoot != null)
                _particlesRoot.gameObject.SetActive(isEnabled);
        }


        protected override void OnRemovedMaterial(byte id, uint count)
        {
            _inventory.AddMaterial(id, count);
            _stats.TilesMined += count;
        }
    }
}