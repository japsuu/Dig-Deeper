using System;
using System.Collections.Generic;
using Singletons;
using UnityEngine;
using WeightedRandomSelector;
using WeightedRandomSelector.Interfaces;
using World.Chunks;

namespace World.Population
{
    public class ChunkPopulationManager : SingletonBehaviour<ChunkPopulationManager>
    {
        [Serializable]
        private class Entry<T>
        {
            public T Object;
            public float Weight = 100;
        }
        
        [SerializeField]
        private List<Entry<GameObject>> _spawnablePrefabs = new();
        
        private IRandomSelector<GameObject> _randomSelector;


        public void Populate(Chunk chunk)
        {
            Vector2 position = chunk.GetRandomPositionInside();

            GameObject prefab = _randomSelector.SelectRandomItem();
            Instantiate(prefab, position, Quaternion.identity, chunk.transform);
        }
        
        
        private void Start()
        {
            _randomSelector = CreateRandomSelector(_spawnablePrefabs);
        }


        private static IRandomSelector<T> CreateRandomSelector<T>(List<Entry<T>> entries)
        {
            DynamicRandomSelector<T> selector = new();
            
            foreach (Entry<T> entry in entries)
            {
                T o = entry.Object;
                if (o != null)
                    selector.Add(o, entry.Weight);
            }

            return selector.Build();
        }
    }
}