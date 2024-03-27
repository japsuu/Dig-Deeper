using UnityEngine;
using World.Chunks;

namespace Entities.Enemies
{
    [RequireComponent(typeof(WormRotation))]
    public class WormHead : WormPart
    {
        [SerializeField]
        private TerrainDigger _terrainDigger;
        
        private WormRotation _rotation;
        private Vector2 _previousPosition;


        protected override void Awake()
        {
            base.Awake();
            
            _rotation = GetComponent<WormRotation>();
            _terrainDigger._scriptUpdateMode = ScriptUpdateMode.Manual;
        }


        protected override void Update()
        {
            base.Update();

            if (_rotation.IsFacingTarget)
                _terrainDigger.ManualUpdate();
        }


        protected override void OnKilled()
        {
            // If the head is killed, the whole worm should be destroyed
            DestroyRecursive();
        }
    }
}