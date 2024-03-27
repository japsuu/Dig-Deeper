using System.Collections.Generic;
using UnityEngine;

namespace Weapons
{
    /// <summary>
    /// Renders a "tracer" for a bullet.
    /// TODO: Fix : Doesn't render at all, when a projectile flies only a short distance.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class RaycastProjectileRenderer : MonoBehaviour
    {
        [SerializeField] private GameObject _hitEffectPrefab;
        
        [SerializeField] private int _tracerPositionBufferSize = 8;

        private LineRenderer _lineRenderer;
        private LinkedList<Vector3> _positionBuffer;
        

        private void Awake()
        {
            _lineRenderer = GetComponent<LineRenderer>();
            _positionBuffer = new LinkedList<Vector3>();
        }


        private void Start()
        {
            // Initialize the lineRenderer.
            _lineRenderer.positionCount = _tracerPositionBufferSize;

            for (int i = 0; i < _tracerPositionBufferSize; i++)
            {
                _lineRenderer.SetPosition(i, transform.position);
            }
        }


        private void Update()
        {            
            _positionBuffer.AddFirst(transform.position);

            // Ensure line length stays as it should.
            if(_positionBuffer.Count > _tracerPositionBufferSize)
                _positionBuffer.RemoveLast();

            // Add positions.
            int index = 0;
            foreach (Vector3 pos in _positionBuffer)
            {
                _lineRenderer.SetPosition(index, pos);
                index++;
            }
        }


        public void OnProjectileHit(RaycastHit2D hit, Vector3 raycastDirection)
        {
            // Spawn the hit effect.
            if (_hitEffectPrefab != null)
            {
                GameObject effect = Instantiate(_hitEffectPrefab, hit.point, Quaternion.identity);
                
                // Rotate the effect according to the direction the bullet came from.
                effect.transform.up = Vector2.Reflect(raycastDirection, hit.normal);
            }
            
            // Detach from projectile to allow the tracer to "finish".
            transform.SetParent(null);
            Destroy(gameObject, 2f);
        }
    }
}