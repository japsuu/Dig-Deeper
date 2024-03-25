using System.Runtime.CompilerServices;
using Materials;
using UnityEngine;

namespace World.Chunks
{
    /// <summary>
    /// Renders a single chunk of pixels.
    /// TODO: Split to ChunkData and ChunkRenderer.
    /// </summary>
    public class Chunk : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _renderer;   //TODO: Change to mesh-renderer and set material main texture instead.
        
        // Flattened 2D arrays for better cache locality.
        private byte[] _tiles;
        private Color[] _pixels;
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
        
        
        public void OnGenerated(TileData[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                _tiles[i] = data[i].ID;
                _pixels[i] = data[i].Color;
            }
            
            _isTextureDirty = true;
            _isGenerated = true;
        }


        private void LateUpdate()
        {
            if (_isGenerated && _isTextureDirty)
                UpdateChunkTexture();
        }


        private void InitializeData()
        {
            _tiles = new byte[Constants.CHUNK_SIZE_PIXELS * Constants.CHUNK_SIZE_PIXELS];
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