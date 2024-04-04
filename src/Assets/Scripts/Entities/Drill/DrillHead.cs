using UnityEngine;
using World.Chunks;

namespace Entities.Drill
{
    /// <summary>
    /// Breaks terrain tiles and collects the materials into a <see cref="DrillInventory"/>.
    /// </summary>
    public class DrillHead : TerrainDigger
    {
        private const int BREAK_INTERVAL_FRAMES_AIRBORNE = 2;   // Higher interval when airborne, since the drill may be moving faster
        private const int BREAK_INTERVAL_FRAMES_NORMAL = 8;
        
        [SerializeField]
        private Transform _particlesRoot;

        private DrillInventory _inventory;
        private DrillStats _stats;

        public bool IsEnabled { get; private set; }


        private void Awake()
        {
            BreakIntervalFrames = BREAK_INTERVAL_FRAMES_AIRBORNE;
        }


        public void Initialize(DrillInventory inventory, DrillStats stats)
        {
            _inventory = inventory;
            _stats = stats;
        }
        
        
        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            BreakIntervalFrames = isEnabled ? BREAK_INTERVAL_FRAMES_NORMAL : BREAK_INTERVAL_FRAMES_AIRBORNE;
            
            if (_particlesRoot != null)
                _particlesRoot.gameObject.SetActive(isEnabled);
        }


        protected override void OnRemovedMaterial(byte id, uint count)
        {
            _inventory.AddMaterial(id, count);
            _stats.TilesMined += (int)count;
        }
    }
}