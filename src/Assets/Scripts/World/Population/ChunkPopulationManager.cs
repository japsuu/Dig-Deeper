using System.Collections.Generic;
using MathHelpers;
using Singletons;
using UnityEngine;
using WeightedRandomSelector.Interfaces;
using World.Chunks;

namespace World.Population
{
    /// <summary>
    /// Manages populating chunks with special objects.
    /// </summary>
    public class ChunkPopulationManager : SingletonBehaviour<ChunkPopulationManager>
    {
        [SerializeField]
        private List<RandomSelectorEntry<GameObject>> _spawnablePrefabs = new();
        
        private IRandomSelector<GameObject> _randomSelector;


        public void Populate(Chunk chunk)
        {
            Vector2 position = chunk.GetRandomPositionInside();

            GameObject prefab = _randomSelector.SelectRandomItem();
            Instantiate(prefab, position, Quaternion.identity, chunk.transform);
        }
        
        
        private void Start()
        {
            _randomSelector = RandomSelectorBuilder.Build(_spawnablePrefabs);
        }
    }
}