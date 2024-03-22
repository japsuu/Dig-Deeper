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
        private float _chunkSizeUnits;
        private int _chunkSizePixels;
        private int _texturePPU;
        private bool _isTextureDirty;
        private bool _isGenerated;
        
        public Vector2Int Position { get; private set; }
        
        
        public void Initialize(int chunkSize, int texturePPU, Vector2Int position)
        {
            _chunkSizePixels = chunkSize;
            _texturePPU = texturePPU;
            _chunkSizeUnits = _chunkSizePixels / (float)_texturePPU;
            Position = position;
            
            InitializeRendering();
            InitializeData();
            UpdateChunkTexture();
        }


        /// <summary>
        /// Sets the tile at the given pixel-position.
        /// </summary>
        public void SetTile(int x, int y, in TileData tile)
        {
            Debug.Assert(x >= 0 && x < _chunkSizePixels, $"X position {x} is out of bounds");
            Debug.Assert(y >= 0 && y < _chunkSizePixels, $"Y position {y} is out of bounds");
            int index = GetArrayIndex(x, y);
            _tiles[index] = tile.ID;
            _pixels[index] = tile.Color;
            _isTextureDirty = true;
        }


        /// <summary>
        /// Sets the tile at the given pixel-position.
        /// </summary>
        public byte GetAndSetTile(int x, int y, in TileData newTile)
        {
            int index = GetArrayIndex(x, y);
            byte oldTile = _tiles[index];
            _tiles[index] = newTile.ID;
            _pixels[index] = newTile.Color;
            _isTextureDirty = true;
            return oldTile;
        }
        
        
        public void OnGenerated()
        {
            _isGenerated = true;
        }


        private void LateUpdate()
        {
            if (_isGenerated && _isTextureDirty)
                UpdateChunkTexture();
        }


        private void InitializeData()
        {
            _tiles = new byte[_chunkSizePixels * _chunkSizePixels];
            _pixels = new Color[_chunkSizePixels * _chunkSizePixels];
        }


        private void InitializeRendering()
        {
            _texture = new Texture2D(_chunkSizePixels, _chunkSizePixels)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            _renderer.sprite = Sprite.Create(_texture, new Rect(0, 0, _chunkSizePixels, _chunkSizePixels), Vector2.zero, _texturePPU);
        }


        private void UpdateChunkTexture()
        {
            _texture.SetPixels(_pixels);
            _texture.Apply();
            _isTextureDirty = false;
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetArrayIndex(int x, int y)
        {
            return y * _chunkSizePixels + x;
        }


        private void OnDrawGizmos()
        {
            float size = _chunkSizeUnits;
            Gizmos.DrawWireCube(transform.position + new Vector3(size / 2f, size / 2f), new Vector3(size, size));
        }
    }
}