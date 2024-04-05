using System.Collections.Generic;
using Materials;
using UnityEngine;
using World.Chunks;

namespace World.Generation
{
    /// <summary>
    /// A "semi"-producer-consumer style generator that generates chunks in the background.
    /// This class acts as the consumer part of the producer-consumer pattern.
    /// Multithreading is skipped for WebGL builds due to Unity's (browsers') limited support for threads.
    /// </summary>
    public class ChunkGenerationManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private MaterialDefinition _baseGroundMaterial;

        [SerializeField]
        private List<MaterialGenerationSettings> _materialGenerationSettings;

        
        [Header("Settings")]
        [SerializeField]
        private int _groundLevel;

        [SerializeField]
        [Tooltip("The height at which chunk population should start.")]
        private int _populationLevel = -50;

        [SerializeField]
        [Range(0f, 1f)]
        private float _caveFactor = 0.25f;

        
        [Header("Noise")]
        [SerializeField] private NoiseFieldSettings _densityNoiseSettings;
        [SerializeField] private NoiseFieldSettings _temperatureSettings;
        [SerializeField] private NoiseFieldSettings _pressureSettings;
        [SerializeField] private NoiseFieldSettings _tectonicsSettings;

        private ChunkProducer _producer;


        public void Initialize(TileDatabase tileDatabase)
        {
            ChunkGenerator generator = new(
                tileDatabase,
                _densityNoiseSettings.GetNoise(),
                _temperatureSettings.GetNoise(),
                _pressureSettings.GetNoise(),
                _tectonicsSettings.GetNoise(),
                _baseGroundMaterial,
                _materialGenerationSettings,
                _groundLevel,
                _populationLevel,
                _caveFactor
            );
            
            #if UNITY_WEBGL
            _producer = new WebGLChunkProducer(generator, this);
            #else
            _producer = new MultithreadedChunkProducer(generator, this);
            #endif
        }


        public void QueueChunkGeneration(Chunk chunk)
        {
            _producer.Post(chunk);
        }


        private void Update()
        {
            _producer.Consume();
        }
    }
}