using System;
using System.Runtime.CompilerServices;
using Materials;
using UI.Settings;
using UnityEngine;
using World.Population;
using Random = UnityEngine.Random;

namespace World.Chunks
{
    /// <summary>
    /// Renders a single chunk of pixels.
    /// TODO: Split to ChunkData and ChunkRenderer.
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _renderer;   //TODO: Change to mesh renderer and set material main texture instead.

        [SerializeField]
        [Tooltip("The chance of this chunk calling the population generator to populate this chunk with some special features.")]
        [Range(0f, 1f)]
        private float _populationChance = 0.002f;
        
        // NOTE: Use flattened 2D arrays for better cache locality.
        private byte[] _tiles;              // Tile IDs.
        private byte[] _hardness;           // Stored separately to quickly get the speed of the drill at a given position. Does not change after generation.
        private Color[] _pixels;            // Pixel colors.
        private Texture2D _texture;
        private bool _isTextureDirty;
        private bool _isGenerated;
        
        public Vector2Int Position { get; private set; }
        
        
        public void Initialize(Vector2Int position)
        {
            Position = position;
            
            InitializeRendering();
            InitializeData();
            UpdateChunkTexture();
        }


        /// <summary>
        /// Gets the tile at the given pixel-position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetTile(int x, int y)
        {
            Debug.Assert(x >= 0 && x < Constants.CHUNK_SIZE_PIXELS, $"X position {x} is out of bounds");
            Debug.Assert(y >= 0 && y < Constants.CHUNK_SIZE_PIXELS, $"Y position {y} is out of bounds");
            return _tiles[GetArrayIndex(x, y)];
        }


        /// <summary>
        /// Gets the movement speed at the given pixel-position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetHardness(int x, int y)
        {
            Debug.Assert(x >= 0 && x < Constants.CHUNK_SIZE_PIXELS, $"X position {x} is out of bounds");
            Debug.Assert(y >= 0 && y < Constants.CHUNK_SIZE_PIXELS, $"Y position {y} is out of bounds");
            return _hardness[GetArrayIndex(x, y)];
        }


        /// <summary>
        /// Sets the tile at the given pixel-position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTile(int x, int y, in TileData tile)
        {
            Debug.Assert(x >= 0 && x < Constants.CHUNK_SIZE_PIXELS, $"X position {x} is out of bounds");
            Debug.Assert(y >= 0 && y < Constants.CHUNK_SIZE_PIXELS, $"Y position {y} is out of bounds");
            int index = GetArrayIndex(x, y);
            _tiles[index] = tile.ID;
            _pixels[index] = tile.Color;
            _isTextureDirty = true;
        }


        /// <summary>
        /// Sets the tile at the given pixel-position.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetAndSetTile(int x, int y, in TileData newTile)
        {
            int index = GetArrayIndex(x, y);
            byte oldTile = _tiles[index];
            _tiles[index] = newTile.ID;
            _pixels[index] = newTile.Color;
            _isTextureDirty = true;
            return oldTile;
        }
        
        
        public void OnGenerated(TileData[] data, bool canPopulate)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _tiles[i] = data[i].ID;
                _hardness[i] = data[i].Hardness;
                _pixels[i] = data[i].Color;
            }
            
            _isTextureDirty = true;
            _isGenerated = true;
            
            if (canPopulate && Random.value < _populationChance * GetPopulationModifier())
                ChunkPopulationManager.Instance.Populate(this);
        }
        
        
        public Vector2 GetRandomPositionInside()
        {
            float x = Random.Range(0, Constants.CHUNK_SIZE_UNITS);
            float y = Random.Range(0, Constants.CHUNK_SIZE_UNITS);
            return transform.position + new Vector3(x, y, 0);
        }


        private void LateUpdate()
        {
            if (_isGenerated && _isTextureDirty)
                UpdateChunkTexture();
        }


        private void InitializeData()
        {
            _tiles = new byte[Constants.CHUNK_SIZE_PIXELS * Constants.CHUNK_SIZE_PIXELS];
            _hardness = new byte[Constants.CHUNK_SIZE_PIXELS * Constants.CHUNK_SIZE_PIXELS];
            _pixels = new Color[Constants.CHUNK_SIZE_PIXELS * Constants.CHUNK_SIZE_PIXELS];
        }


        private void InitializeRendering()
        {
            _texture = new Texture2D(Constants.CHUNK_SIZE_PIXELS, Constants.CHUNK_SIZE_PIXELS)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _renderer.sprite = Sprite.Create(_texture, new Rect(0, 0, Constants.CHUNK_SIZE_PIXELS, Constants.CHUNK_SIZE_PIXELS), Vector2.zero, Constants.TEXTURE_PPU);
        }


        private void UpdateChunkTexture()
        {
            _texture.SetPixels(_pixels);
            _texture.Apply();
            _isTextureDirty = false;
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetArrayIndex(int x, int y) => y * Constants.CHUNK_SIZE_PIXELS + x;
        
        
        private static float GetPopulationModifier()
        {
            return DifficultySettings.CurrentDifficulty switch
            {
                DifficultySettings.Difficulty.Easy => 0.5f,
                DifficultySettings.Difficulty.Normal => 1f,
                DifficultySettings.Difficulty.Mayhem => 3f,
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(
                transform.position + new Vector3(Constants.CHUNK_SIZE_UNITS / 2f, Constants.CHUNK_SIZE_UNITS / 2f),
                new Vector3(Constants.CHUNK_SIZE_UNITS, Constants.CHUNK_SIZE_UNITS));
        }
    }
}