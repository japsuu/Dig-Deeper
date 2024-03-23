using System;
using UnityEngine;

namespace World.Chunks
{
    /// <summary>
    /// Component that removes terrain tiles in a radius around it.
    /// </summary>
    public class TerrainDigger : MonoBehaviour
    {
        [SerializeField]
        protected int BreakRadius = 10;
        
        [SerializeField]
        protected int BreakIntervalFrames = 10;   // We do not need to break tiles every frame.
        
        private int _breakIntervalFrameCounter;
        
        
        protected virtual void Update()
        {
            BreakTiles();
        }


        protected virtual void BreakTiles()
        {
            if (_breakIntervalFrameCounter++ < BreakIntervalFrames)
                return;
            
            Span<uint> replacedTiles = stackalloc uint[ChunkManager.Instance.RegisteredTileCount];
            ChunkManager.Instance.BreakTilesInRange(replacedTiles, transform.position, BreakRadius);
            
            for (int i = 1; i < replacedTiles.Length; i++)  // Start at 1 to skip air.
            {
                OnRemovedMaterial((byte)i, replacedTiles[i]);
            }
            
            _breakIntervalFrameCounter = 0;
        }
        
        
        protected virtual void OnRemovedMaterial(byte id, uint count)
        {
            
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            float radiusUnits = BreakRadius / (float)Constants.TEXTURE_PPU;
            Gizmos.DrawWireSphere(transform.position, radiusUnits);
        }
    }
}