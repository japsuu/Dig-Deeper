using System;
using Entities.Drill;
using JetBrains.Annotations;
using Singletons;
using UnityEngine;

namespace World.Stations
{
    /// <summary>
    /// Manages the spawning/despawning of trading stations.
    /// Only a single trading station can exist at a time.
    /// </summary>
    public class TradingStationManager : SingletonBehaviour<TradingStationManager>
    {
        public static event Action StationCreated;
        public static event Action StationDeleted;
        
        [CanBeNull]
        public static TradingStation StationInstance;
        
        [SerializeField]
        private TradingStation _stationPrefab;


        private void Update()
        {
            const float stationInterval = Constants.STATION_DEPTH_INTERVAL;

            Vector2 playerPosition = DrillController.Instance.transform.position;
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
            StationInstance = Instantiate(_stationPrefab, nextStationPos, Quaternion.identity);
            StationCreated?.Invoke();
        }


        private void DespawnStation()
        {
            if (StationInstance == null)
                return;
            Destroy(StationInstance.gameObject);
            StationInstance = null;
            StationDeleted?.Invoke();
        }
    }
}