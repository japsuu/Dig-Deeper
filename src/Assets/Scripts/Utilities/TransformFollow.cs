using UnityEngine;

namespace Utilities
{
    public class TransformFollow : MonoBehaviour
    {
        [SerializeField]
        private Transform _target;
        
        
        private void Update()
        {
            transform.position = _target.position;
        }
    }
}