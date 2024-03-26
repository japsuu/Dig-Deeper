using World.Chunks;

namespace Entities.Drill
{
    public class DrillHead : TerrainDigger
    {
        private const int BREAK_INTERVAL_FRAMES_CUTSCENE = 2;
        private const int BREAK_INTERVAL_FRAMES = 8;

        private DrillInventory _inventory;

        public bool IsEnabled { get; private set; }


        private void Awake()
        {
            BreakIntervalFrames = BREAK_INTERVAL_FRAMES_CUTSCENE;
        }


        public void Initialize(DrillInventory inventory)
        {
            _inventory = inventory;
        }
        
        
        public void SetEnabled(bool isEnabled)
        {
            IsEnabled = isEnabled;
            BreakIntervalFrames = isEnabled ? BREAK_INTERVAL_FRAMES : BREAK_INTERVAL_FRAMES_CUTSCENE;
        }


        protected override void OnRemovedMaterial(byte id, uint count)
        {
            _inventory.AddMaterial(id, count);
        }
    }
}