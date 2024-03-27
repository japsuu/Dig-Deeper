using UnityEngine;

namespace World
{
    /// <summary>
    /// Destroys the object after a certain amount of time.
    /// Creates an effect when destroyed.
    /// </summary>
    public class DelayedDestroy : MonoBehaviour
    {
        [SerializeField]
        private float _lifetime = 10f;
        
        [SerializeField]
        private GameObject _destroyEffect;
        
        private float _timer;
        
        
        public void Initialize(float lifetime, GameObject destroyEffect)
        {
            _lifetime = lifetime;
            _destroyEffect = destroyEffect;
        }
        
        
        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer < _lifetime)
                return;
            
            if (_destroyEffect != null)
                Instantiate(_destroyEffect, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}