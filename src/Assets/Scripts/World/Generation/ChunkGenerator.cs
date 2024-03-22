using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Materials;
using NaughtyAttributes;
using Thirdparty;
using UnityEngine;
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

        [Header("Noise")]
        [SerializeField]
        private NoiseFieldSettings _caveNoiseSettings;

        [SerializeField]
        private NoiseFieldSettings _noise1Settings;

        [SerializeField]
        private NoiseFieldSettings _noise2Settings;

        [SerializeField]
        private NoiseFieldSettings _noise3Settings;

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
        private FastNoiseLite _noise0;
        private FastNoiseLite _noise1;
        private FastNoiseLite _noise2;
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
                    float noise0C00 = GetNoiseValue(_noise0, brickXWorld, brickYWorld);
                    float noise0C10 = GetNoiseValue(_noise0, brickWorldXExt, brickYWorld);
                    float noise0C01 = GetNoiseValue(_noise0, brickXWorld, brickWorldYExt);
                    float noise0C11 = GetNoiseValue(_noise0, brickWorldXExt, brickWorldYExt);
                    float noise1C00 = GetNoiseValue(_noise1, brickXWorld, brickYWorld);
                    float noise1C10 = GetNoiseValue(_noise1, brickWorldXExt, brickYWorld);
                    float noise1C01 = GetNoiseValue(_noise1, brickXWorld, brickWorldYExt);
                    float noise1C11 = GetNoiseValue(_noise1, brickWorldXExt, brickWorldYExt);
                    float noise2C00 = GetNoiseValue(_noise2, brickXWorld, brickYWorld);
                    float noise2C10 = GetNoiseValue(_noise2, brickWorldXExt, brickYWorld);
                    float noise2C01 = GetNoiseValue(_noise2, brickXWorld, brickWorldYExt);
                    float noise2C11 = GetNoiseValue(_noise2, brickWorldXExt, brickWorldYExt);

                    // Process each block within the brick.
                    for (int blockY = 0; blockY < NOISE_SAMPLE_BRICK_SIZE; blockY++)
                    {
                        for (int blockX = 0; blockX < NOISE_SAMPLE_BRICK_SIZE; blockX++)
                        {
                            // Calculate the relative position of the block within its brick.
                            float rx = blockX / (float)NOISE_SAMPLE_BRICK_SIZE;
                            float ry = blockY / (float)NOISE_SAMPLE_BRICK_SIZE;

                            // Interpolate the noise values for the block's position.
                            float density = InterpolateNoise(
                                densityC00,
                                densityC10,
                                densityC01,
                                densityC11,
                                rx, ry);
                            float noise0 = InterpolateNoise(
                                noise0C00,
                                noise0C10,
                                noise0C01,
                                noise0C11,
                                rx, ry);
                            float noise1 = InterpolateNoise(
                                noise1C00,
                                noise1C10,
                                noise1C01,
                                noise1C11,
                                rx, ry);
                            float noise2 = InterpolateNoise(
                                noise2C00,
                                noise2C10,
                                noise2C01,
                                noise2C11,
                                rx, ry);

                            int chunkRelativeX = brickXLocal + blockX;
                            int chunkRelativeY = brickYLocal + blockY;

                            GenerateTerrain(chunk, chunkRelativeX, chunkRelativeY, density, noise0, noise1, noise2);
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


        private void GenerateTerrain(Chunk chunk, int chunkRelativeX, int chunkRelativeY, float density, float noise0, float noise1, float noise2)
        {
            // Carve caves.
            if (density < _caveFactor)
                return;

            byte tileId = GetTileAt(chunkRelativeX, chunkRelativeY, noise0, noise1, noise2);
            TileData tileData = _tileDatabase.TileData[tileId];

            chunk.SetTile(chunkRelativeX, chunkRelativeY, tileData);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte GetTileAt(int x, int y, float n0, float n1, float n2)
        {
            foreach (MaterialGenerationSettings s in _materialGenerationSettings)
            {
                if (s.Threshold1 < n0)
                    continue;

                if (s.Threshold2 < n1)
                    continue;

                if (s.Threshold3 < n2)
                    continue;

                return s.Material.DynamicId;
            }

            return _baseGroundMaterial.DynamicId;
        }


        private void InitializeNoise()
        {
            _caveNoise = _caveNoiseSettings.GetNoise();
            _noise0 = _noise1Settings.GetNoise();
            _noise1 = _noise2Settings.GetNoise();
            _noise2 = _noise3Settings.GetNoise();
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

                float n0 = GetNoiseValue(_noise0, tileWorldX, tileWorldY);
                float n1 = GetNoiseValue(_noise1, tileWorldX, tileWorldY);
                float n2 = GetNoiseValue(_noise2, tileWorldX, tileWorldY);

                if (_debugSettings != null)
                {
                    if (_debugSettings.Threshold1 < n0)
                        continue;

                    if (_debugSettings.Threshold2 < n1)
                        continue;

                    if (_debugSettings.Threshold3 < n2)
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