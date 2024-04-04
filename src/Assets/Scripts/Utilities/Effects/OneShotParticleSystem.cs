using UnityEngine;

namespace Utilities.Effects
{
    /// <summary>
    /// Attach to gameObject to destroy when a child particle system is finished playing.
    /// </summary>
    public class OneShotParticleSystem : MonoBehaviour
    {
        private ParticleSystem _particleSystem;


        private void Awake()
        {
            _particleSystem = GetComponentInChildren<ParticleSystem>();
        }


        private void Start()
        {
            float destroyAfterSeconds = _particleSystem.main.duration;
            
            Destroy(gameObject, destroyAfterSeconds);
        }
    }
}