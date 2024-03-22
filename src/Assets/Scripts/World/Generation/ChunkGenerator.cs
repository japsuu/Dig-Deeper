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
        private FastNoiseLite _noise1;
        private FastNoiseLite _noise2;
        private FastNoiseLite _noise3;
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


        private void GenerateChunk(Chunk chunk) //TODO: Optimize with bilinear sampling.
        {
            int chunkX = chunk.Position.x; // The world position of the chunk.
            int chunkY = chunk.Position.y;

            if (chunkY > _groundLevel)
                return;

            for (int x = 0; x < Constants.CHUNK_SIZE_PIXELS; x++)
            for (int y = 0; y < Constants.CHUNK_SIZE_PIXELS; y++)
            {
                float pixelX = chunkX * Constants.TEXTURE_PPU + x;
                float pixelY = chunkY * Constants.TEXTURE_PPU + y;
                
                // Carve caves.
                float density = GetNoiseValue(_caveNoise, pixelX, pixelY);
                if (density < _caveFactor)
                    continue;
                
                byte tileId = GetTileAt(pixelX, pixelY);
                TileData tileData = _tileDatabase.TileData[tileId];
                
                chunk.SetTile(x, y, tileData);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte GetTileAt(float x, float y)
        {
            float n0 = GetNoiseValue(_noise1, x, y);
            float n1 = GetNoiseValue(_noise2, x, y);
            float n2 = GetNoiseValue(_noise3, x, y);

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
            _noise1 = _noise1Settings.GetNoise();
            _noise2 = _noise2Settings.GetNoise();
            _noise3 = _noise3Settings.GetNoise();
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
            {
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
                    float n0 = GetNoiseValue(_noise1, tileWorldX, tileWorldY);
                    float n1 = GetNoiseValue(_noise2, tileWorldX, tileWorldY);
                    float n2 = GetNoiseValue(_noise3, tileWorldX, tileWorldY);

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