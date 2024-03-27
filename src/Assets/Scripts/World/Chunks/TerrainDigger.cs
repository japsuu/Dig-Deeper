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
        
        [SerializeField]
        public ScriptUpdateMode _scriptUpdateMode = ScriptUpdateMode.Update;

        [SerializeField]
        [Tooltip("Whether to break all tiles between the current and previous position. More expensive, but smoother.")]
        private bool _enableLineCasting;
        
        private Vector3 _previousPosition;
        private int _breakIntervalFrameCounter;


        private void Awake()
        {
            _previousPosition = transform.position;
        }


        protected virtual void Update()
        {
            if (_scriptUpdateMode == ScriptUpdateMode.Update)
                BreakTiles();
        }
        
        
        protected virtual void FixedUpdate()
        {
            if (_scriptUpdateMode == ScriptUpdateMode.FixedUpdate)
                BreakTiles();
        }
        
        
        public void ManualUpdate()
        {
            if (_scriptUpdateMode == ScriptUpdateMode.Manual)
                BreakTiles();
        }


        protected virtual void BreakTiles()
        {
            if (_breakIntervalFrameCounter++ < BreakIntervalFrames)
                return;
            
            Span<uint> replacedTiles = stackalloc uint[ChunkManager.Instance.RegisteredTileCount];
            
            if (_enableLineCasting)
            {
                ChunkManager.Instance.GetAndSetTilesInLine(replacedTiles, _previousPosition, transform.position, BreakRadius);
            }
            else
            {
                ChunkManager.Instance.GetAndSetTilesInRange(replacedTiles, transform.position, BreakRadius);
            }
            
            for (int i = 1; i < replacedTiles.Length; i++)  // Start at 1 to skip air.
            {
                OnRemovedMaterial((byte)i, replacedTiles[i]);
            }
            
            _breakIntervalFrameCounter = 0;
            _previousPosition = transform.position;
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