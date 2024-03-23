using System;
using UnityEngine;

namespace World.Chunks
{
    /// <summary>
    /// Component that detects terrain tiles in a radius around it.
    /// </summary>
    public class TerrainTrigger : MonoBehaviour
    {
        public event Action ContactStart;
        public event Action ContactEnd;
        
        [SerializeField]
        protected int CheckRadius = 5;
        
        [SerializeField]
        protected int CheckIntervalFrames = 4;   // We do not need to check tiles every frame.
        
        private int _checkIntervalFrameCounter;
        
        public bool IsTriggered { get; private set; }
        
        
        protected virtual void LateUpdate()
        {
            CheckTiles();
        }


        protected virtual void CheckTiles()
        {
            if (_checkIntervalFrameCounter++ < CheckIntervalFrames)
                return;
            
            bool triggered = ChunkManager.Instance.ContainsNonAirTilesInRange( transform.position, CheckRadius);
            if (triggered && !IsTriggered)
            {
                ContactStart?.Invoke();
                IsTriggered = true;
            }
            else if (!triggered && IsTriggered)
            {
                ContactEnd?.Invoke();
                IsTriggered = false;
            }
            
            _checkIntervalFrameCounter = 0;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            float radiusUnits = CheckRadius / (float)Constants.TEXTURE_PPU;
            Gizmos.DrawWireSphere(transform.position, radiusUnits);
        }
    }
}