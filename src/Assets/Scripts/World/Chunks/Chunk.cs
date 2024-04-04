using System.Runtime.CompilerServices;
using Materials;
using UnityEngine;
using World.Population;
using Random = UnityEngine.Random;

namespace World.Chunks
{
    /// <summary>
    /// Contains a single chunk of world tile data.
    /// TODO: Split to ChunkData and ChunkRenderer.
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _renderer;

        [SerializeField]
        [Tooltip("The chance of this chunk calling the population generator to populate this chunk with some special features.")]
        [Range(0f, 1f)]
        private float _populationChance = 0.002f;
        
        // Individual tiles are queried infrequently, allowing us to store the individual tile data entries separately in flattened arrays, instead of storing the actual TileData.
        // This allows us to improve cache locality.
        // If a full tile data object is requested, we reconstruct it from the individual data.
        private byte[] _tileIDs;
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
            return _tileIDs[GetArrayIndex(x, y)];
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
            _tileIDs[index] = tile.ID;
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
            byte oldTile = _tileIDs[index];
            _tileIDs[index] = newTile.ID;
            _pixels[index] = newTile.Color;
            _isTextureDirty = true;
            return oldTile;
        }
        
        
        public void OnGenerated(TileData[] data, bool canPopulate)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _tileIDs[i] = data[i].ID;
                _hardness[i] = data[i].Hardness;
                _pixels[i] = data[i].Color;
            }
            
            _isTextureDirty = true;
            _isGenerated = true;
            
            if (canPopulate && Random.value < _populationChance * Difficulty.GetChunkPopulationModifier())
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
            _tileIDs = new byte[Constants.CHUNK_SIZE_PIXELS * Constants.CHUNK_SIZE_PIXELS];
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


        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(
                transform.position + new Vector3(Constants.CHUNK_SIZE_UNITS / 2f, Constants.CHUNK_SIZE_UNITS / 2f),
                new Vector3(Constants.CHUNK_SIZE_UNITS, Constants.CHUNK_SIZE_UNITS));
        }
    }
}