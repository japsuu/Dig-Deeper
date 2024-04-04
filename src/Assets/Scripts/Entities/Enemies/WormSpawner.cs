using NaughtyAttributes;
using UnityEngine;
using World.Chunks;
using World.Population;

namespace Entities.Enemies
{
    /// <summary>
    /// Creates a single worm entity.
    /// </summary>
    public class WormSpawner : MonoBehaviour
    {
        [SerializeField]
        private WormHead _headPrefab;

        [SerializeField]
        private WormBody _bodyPrefab;

        [SerializeField]
        private int _bodyPartsCount = 5;

        [SerializeField]
        private int _maxHealth = 100;

        [SerializeField]
        private float _headReceivedDamageMultiplier = 1f;

        [SerializeField]
        private float _bodyReceivedDamageMultiplier = 0.5f;

        [SerializeField]
        private bool _spawnOnStart = true;


        private void Start()
        {
            if (_spawnOnStart)
                SpawnWorm();
        }


        public void SpawnWormDelayed(float delay)
        {
            Invoke(nameof(SpawnWorm), delay);
        }


        [Button("Spawn Default Worm")]
        public WormHead SpawnWorm(bool destroySelf = true)
        {
            WormHead head = Instantiate(_headPrefab, transform.position, Quaternion.identity);
            head.gameObject.name = "Worm Head";
            head.SetMaxHealth(_maxHealth, true);
            head.SetReceivedDamageMultiplier(_headReceivedDamageMultiplier);

            WormPart previousPart = head;
            for (int i = 0; i < _bodyPartsCount; i++)
            {
                WormBody part = Instantiate(_bodyPrefab, transform.position, Quaternion.identity);
                part.gameObject.name = $"Worm Body {i}";
                part.HeadRef = head;
                part.SetOrderInLayer(previousPart.SpriteRenderer.sortingOrder - 1);
                part.SetMaxHealth(_maxHealth, true);
                part.SetReceivedDamageMultiplier(_bodyReceivedDamageMultiplier);
                previousPart.SetTailLink(part);

                previousPart = part;
            }

            if (destroySelf && Application.isPlaying)
                Destroy(gameObject);

            return head;
        }
    }
}