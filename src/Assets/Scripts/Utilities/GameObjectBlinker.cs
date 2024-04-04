using UnityEngine;

namespace Utilities
{
    public class GameObjectBlinker : MonoBehaviour
    {
        [SerializeField]
        private GameObject _object;
        
        [SerializeField]
        private float _blinkInterval = 1f;
        
        [SerializeField]
        private float _blinkLength = 0.2f;
        
        private float _blinkOffTimer;
        private float _blinkOnTimer;
        
        
        private void Update()
        {
            _blinkOffTimer -= Time.deltaTime;
            _blinkOnTimer -= Time.deltaTime;
            
            if (_blinkOffTimer <= 0)
            {
                _object.SetActive(true);
                _blinkOnTimer = _blinkLength;
                _blinkOffTimer = _blinkInterval;
            }
            else if (_blinkOnTimer <= 0)
            {
                _object.SetActive(false);
            }
        }
    }
}