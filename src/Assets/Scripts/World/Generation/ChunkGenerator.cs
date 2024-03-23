using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Materials;
using NaughtyAttributes;
using Thirdparty;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using World.Chunks;

namespace World.Generation
{
    public class ChunkGenerator : MonoBehaviour
    {
        private const int MAX_CHUNKS_GENERATED_PER_FRAME = 3;

        // Optimization constants.
        private const int NOISE_SAMPLE_BRICK_SIZE = 4;
        private const int BRICK_COUNT_X = Constants.CHUNK_SIZE_PIXELS / NOISE_SAMPLE_BRICK_SIZE;
        private const int BRICK_COUNT_Y = Constants.CHUNK_SIZE_PIXELS / NOISE_SAMPLE_BRICK_SIZE;

        [SerializeField]
        private MaterialDefinition _baseGroundMaterial;

        [SerializeField]
        private List<MaterialGenerationSettings> _materialGenerationSettings;

        [SerializeField]
        private int _groundLevel;

        [FormerlySerializedAs("_caveNoiseSettings")]
        [Header("Noise")]
        [SerializeField]
        private NoiseFieldSettings _densityNoiseSettings;

        [FormerlySerializedAs("_noise1Settings")]
        [SerializeField]
        private NoiseFieldSettings _temperatureSettings;

        [FormerlySerializedAs("_noise2Settings")]
        [SerializeField]
        private NoiseFieldSettings _pressureSettings;

        [FormerlySerializedAs("_noise3Settings")]
        [SerializeField]
        private NoiseFieldSettings _tectonicsSettings;

        [SerializeField]
        [Range(0f, 1f)]
        private float _caveFactor = 0.25f;

#if UNITY_EDITOR
        [SerializeField]
        private Image _debugImage;

        [SerializeField]
        private MaterialGenerationSettings _debugSettings;
#endif

        private TileDatabase _tileDatabase;
        private FastNoiseLite _caveNoise;
        private FastNoiseLite _temperatureNoise;
        private FastNoiseLite _pressureNoise;
        private FastNoiseLite _tectonicsNoise;
        private Queue<Chunk> _chunksToGenerate;


        public void Initialize(TileDatabase tileDatabase)
        {
            _tileDatabase = tileDatabase;
            _chunksToGenerate = new Queue<Chunk>();
            InitializeNoise();
        }


        public void QueueChunkGeneration(Chunk chunk)
        {
            _chunksToGenerate.Enqueue(chunk);
        }


        private void Update()
        {
            ProcessQueues();
        }


        private void ProcessQueues()
        {
            int chunksGenerated = 0;
            while (_chunksToGenerate.Count > 0 && chunksGenerated < MAX_CHUNKS_GENERATED_PER_FRAME)
            {
                Chunk chunk = _chunksToGenerate.Dequeue();
                if (chunk == null)
                    continue;
                GenerateChunk(chunk);
                chunk.OnGenerated();
                chunksGenerated++;
            }
        }


        private void GenerateChunk(Chunk chunk)
        {
            int chunkXPixels = chunk.Position.x * Constants.TEXTURE_PPU;
            int chunkYPixels = chunk.Position.y * Constants.TEXTURE_PPU;

            if (chunkYPixels > _groundLevel)
                return;

            // Divide the chunk in to "bricks" to optimize noise sampling by taking less samples.
            // Each brick corner is sampled for noise, and further values are interpolated from them using trilinear interpolation.
            for (int brickYIndex = 0; brickYIndex < BRICK_COUNT_Y; brickYIndex++)
            {
                int brickYLocal = brickYIndex * NOISE_SAMPLE_BRICK_SIZE;
                int brickYWorld = chunkYPixels + brickYLocal;
                for (int brickXIndex = 0; brickXIndex < BRICK_COUNT_X; brickXIndex++)
                {
                    int brickXLocal = brickXIndex * NOISE_SAMPLE_BRICK_SIZE;
                    int brickXWorld = chunkXPixels + brickXLocal;

                    // Sample the noise at each corner of the brick.
                    int brickWorldXExt = brickXWorld + NOISE_SAMPLE_BRICK_SIZE;
                    int brickWorldYExt = brickYWorld + NOISE_SAMPLE_BRICK_SIZE;
                    float densityC00 = GetNoiseValue(_caveNoise, brickXWorld, brickYWorld);
                    float densityC10 = GetNoiseValue(_caveNoise, brickWorldXExt, brickYWorld);
                    float densityC01 = GetNoiseValue(_caveNoise, brickXWorld, brickWorldYExt);
                    float densityC11 = GetNoiseValue(_caveNoise, brickWorldXExt, brickWorldYExt);
                    float temperatureC00 = GetNoiseValue(_temperatureNoise, brickXWorld, brickYWorld);
                    float temperature0C10 = GetNoiseValue(_temperatureNoise, brickWorldXExt, brickYWorld);
                    float temperature0C01 = GetNoiseValue(_temperatureNoise, brickXWorld, brickWorldYExt);
                    float temperature0C11 = GetNoiseValue(_temperatureNoise, brickWorldXExt, brickWorldYExt);
                    float pressureC00 = GetNoiseValue(_pressureNoise, brickXWorld, brickYWorld);
                    float pressureC10 = GetNoiseValue(_pressureNoise, brickWorldXExt, brickYWorld);
                    float pressureC01 = GetNoiseValue(_pressureNoise, brickXWorld, brickWorldYExt);
                    float pressureC11 = GetNoiseValue(_pressureNoise, brickWorldXExt, brickWorldYExt);
                    float tectonicsC00 = GetNoiseValue(_tectonicsNoise, brickXWorld, brickYWorld);
                    float tectonicsC10 = GetNoiseValue(_tectonicsNoise, brickWorldXExt, brickYWorld);
                    float tectonicsC01 = GetNoiseValue(_tectonicsNoise, brickXWorld, brickWorldYExt);
                    float tectonicsC11 = GetNoiseValue(_tectonicsNoise, brickWorldXExt, brickWorldYExt);

                    // Process each pixel within the brick.
                    for (int pixelY = 0; pixelY < NOISE_SAMPLE_BRICK_SIZE; pixelY++)
                    {
                        for (int pixelX = 0; pixelX < NOISE_SAMPLE_BRICK_SIZE; pixelX++)
                        {
                            // Calculate the relative position of the pixel within its brick.
                            float rx = pixelX / (float)NOISE_SAMPLE_BRICK_SIZE;
                            float ry = pixelY / (float)NOISE_SAMPLE_BRICK_SIZE;

                            // Interpolate the noise values for the pixel's position.
                            float density = InterpolateNoise(
                                densityC00,
                                densityC10,
                                densityC01,
                                densityC11,
                                rx, ry);
                            float temperature = InterpolateNoise(
                                temperatureC00,
                                temperature0C10,
                                temperature0C01,
                                temperature0C11,
                                rx, ry);
                            float pressure = InterpolateNoise(
                                pressureC00,
                                pressureC10,
                                pressureC01,
                                pressureC11,
                                rx, ry);
                            float tectonics = InterpolateNoise(
                                tectonicsC00,
                                tectonicsC10,
                                tectonicsC01,
                                tectonicsC11,
                                rx, ry);

                            int chunkRelativeX = brickXLocal + pixelX;
                            int chunkRelativeY = brickYLocal + pixelY;

                            GenerateTerrain(chunk, chunkRelativeX, chunkRelativeY, density, temperature, pressure, tectonics);
                        }
                    }
                }
            }
        }


        private float InterpolateNoise(float c00, float c10, float c01, float c11, float x, float y)
        {
            // Simple bi-linear interpolation.
            float c0 = c00 * (1 - x) + c10 * x;
            float c1 = c01 * (1 - x) + c11 * x;

            return c0 * (1 - y) + c1 * y;
        }


        private void GenerateTerrain(Chunk chunk, int chunkRelativeX, int chunkRelativeY, float density, float temperature, float pressure, float tectonics)
        {
            // Carve caves.
            if (density < _caveFactor)
                return;

            byte tileId = GetTileAt(chunkRelativeX, chunkRelativeY, temperature, pressure, tectonics);
            TileData tileData = _tileDatabase.TileData[tileId];

            chunk.SetTile(chunkRelativeX, chunkRelativeY, tileData);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte GetTileAt(int x, int y, float temp, float press, float tect)
        {
            foreach (MaterialGenerationSettings s in _materialGenerationSettings)
            {
                if (s.TemperatureThreshold < temp)
                    continue;

                if (s.PressureThreshold < press)
                    continue;

                if (s.TectonicsThreshold < tect)
                    continue;

                return s.Material.DynamicId;
            }

            return _baseGroundMaterial.DynamicId;
        }


        private void InitializeNoise()
        {
            _caveNoise = _densityNoiseSettings.GetNoise();
            _temperatureNoise = _temperatureSettings.GetNoise();
            _pressureNoise = _pressureSettings.GetNoise();
            _tectonicsNoise = _tectonicsSettings.GetNoise();
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float GetNoiseValue(FastNoiseLite fnl, float x, float y) => (fnl.GetNoise(x, y) + 1) / 2;


#if UNITY_EDITOR
        [Button("Generate Debug Texture")]
        private void GenerateAndShowDebugTexture()
        {
            InitializeNoise();

            int width = Constants.TEXTURE_PPU * (int)_debugImage.rectTransform.rect.width;
            int height = Constants.TEXTURE_PPU * (int)_debugImage.rectTransform.rect.height;
            Texture2D texture = new(width, height)
            {
                filterMode = FilterMode.Point
            };

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                float tileWorldX = x;
                float tileWorldY = y;

                float density = GetNoiseValue(_caveNoise, tileWorldX, tileWorldY);
                if (density < _caveFactor)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float n0 = GetNoiseValue(_temperatureNoise, tileWorldX, tileWorldY);
                float n1 = GetNoiseValue(_pressureNoise, tileWorldX, tileWorldY);
                float n2 = GetNoiseValue(_tectonicsNoise, tileWorldX, tileWorldY);

                if (_debugSettings != null)
                {
                    if (_debugSettings.TemperatureThreshold < n0)
                        continue;

                    if (_debugSettings.PressureThreshold < n1)
                        continue;

                    if (_debugSettings.TectonicsThreshold < n2)
                        continue;

                    texture.SetPixel(x, y, new Color(1f, 0f, 1f, 1f));
                }
                else
                {
                    texture.SetPixel(x, y, new Color(n0, n1, n2, 1f));
                }
            }

            texture.Apply();
            _debugImage.gameObject.transform.root.gameObject.SetActive(true);
            _debugImage.sprite = Sprite.Create(texture, new Rect(0, 0, width, height), Vector2.zero);
        }


        [Button("Destroy Debug Texture")]
        private void DestroyDebugTexture()
        {
            _debugImage.sprite = null;
            _debugImage.gameObject.transform.root.gameObject.SetActive(false);
        }
#endif
    }
}