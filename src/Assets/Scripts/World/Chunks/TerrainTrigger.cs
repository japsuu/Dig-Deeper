using System;
using NaughtyAttributes;
using UnityEngine;

namespace World.Chunks
{
    /// <summary>
    /// Component that detects terrain tiles in a radius around it.
    /// </summary>
    public class TerrainTrigger : CustomBehaviour
    {
        public event Action ContactStart;
        public event Action ContactEnd;
        
        [SerializeField]
        protected int CheckRadius = 5;
        
        [SerializeField, ReadOnly]
        public bool IsTriggered;


        private void UpdateTrigger()
        {
            bool triggered = ChunkManager.Instance.ContainsNonAirTilesInRange( transform.position, CheckRadius);
            bool triggerStart = triggered && !IsTriggered;
            bool triggerStop = !triggered && IsTriggered;
            IsTriggered = triggered;
            
            if (triggerStart)
                ContactStart?.Invoke();
            else if (triggerStop)
                ContactEnd?.Invoke();
        }


        protected override void InternalUpdate()
        {
            UpdateTrigger();
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            float radiusUnits = CheckRadius / (float)Constants.TEXTURE_PPU;
            Gizmos.DrawWireSphere(transform.position, radiusUnits);
        }
    }
}