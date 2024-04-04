using System.Collections.Generic;
using Entities.Drill;
using JetBrains.Annotations;
using MathHelpers;
using Singletons;
using UnityEngine;
using WeightedRandomSelector.Interfaces;

namespace World.Stations
{
    /// <summary>
    /// Manages the spawning/despawning of trading stations.
    /// Only a single trading station can exist at a time.
    /// </summary>
    public class TradingStationManager : SingletonBehaviour<TradingStationManager>
    {
        [CanBeNull]
        public static TradingStation StationInstance;
        
        [SerializeField]
        private List<RandomSelectorEntry<TradingStation>> _spawnablePrefabs = new();
        
        private IRandomSelector<TradingStation> _randomSelector;
        
        
        private void Start()
        {
            _randomSelector = RandomSelectorBuilder.Build(_spawnablePrefabs);
        }


        private void Update()
        {
            const float stationInterval = Constants.STATION_DEPTH_INTERVAL;

            Vector2 playerPosition = DrillStateMachine.Instance.transform.position;
            float playerDepth = -playerPosition.y;
            float depthProgress = playerDepth % stationInterval;
            float stationProgress = depthProgress / stationInterval;
            
            if (StationInstance == null)
            {
                if (stationProgress >= 0.7f)
                {
                    float depth = Mathf.CeilToInt(playerDepth / stationInterval) * stationInterval;
                    Vector2 nextStationPos = new Vector2(playerPosition.x, -depth);
                    SpawnStation(nextStationPos);
                }
            }
            else
            {
                if (stationProgress >= 0.2f && stationProgress < 0.3f)
                    DespawnStation();   // Scuffed hack to despawn the station when the player is 20% through it
            }
        }


        private void SpawnStation(Vector2 nextStationPos)
        {
            StationInstance = Instantiate(_randomSelector.SelectRandomItem(), nextStationPos, Quaternion.identity);
            EventManager.TradingStations.OnStationCreated();
        }


        private void DespawnStation()
        {
            if (StationInstance == null)
                return;
            Destroy(StationInstance.gameObject);
            StationInstance = null;
            EventManager.TradingStations.OnStationDeleted();
        }
    }
}