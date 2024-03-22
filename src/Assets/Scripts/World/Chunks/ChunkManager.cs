using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Drill;
using Materials;
using Singletons;
using UnityEngine;
using World.Generation;

namespace World.Chunks
{
    /// <summary>
    /// Manages chunk loading/unloading.
    /// TODO: Handle origin shifting if the player digs too deep, or just seamlessly teleport them to the top of the map.
    /// TODO: Refactor this class to smaller parts.
    /// </summary>
    [RequireComponent(typeof(ChunkManager))]
    public class ChunkManager : SingletonBehaviour<ChunkManager>
    {
        [SerializeField]
        private MaterialCollection _availableMaterials;
        
        [SerializeField]
        private Chunk _chunkPrefab;
        
        [SerializeField]
        private int _chunkLoadRadius = 6;

        private int _chunkLoadRadiusSquared;
        private int _chunkUnloadRadiusSquared;
        private Vector2Int _chunkCenterOffset;
        private ChunkGenerator _chunkGenerator;
        private Vector2Int _playerChunkPosition;
        private List<Vector2Int> _chunkLoadSpiral;  // Precomputed spiral of chunk positions to load.
        private TileDatabase _tileDatabase;
        private readonly Chunk[] _chunkNeighbourhoodBuffer = new Chunk[9];
        private readonly HashSet<Vector2Int> _loadedChunks = new();
        private readonly Dictionary<Vector2Int, Chunk> _loadedChunksMap = new();
        
        public int RegisteredTileCount => _tileDatabase.RegisteredTileCount;
        
        
        private void Awake()
        {
            _chunkGenerator = GetComponent<ChunkGenerator>();
            _chunkLoadRadiusSquared = _chunkLoadRadius * _chunkLoadRadius;
            // Extend the unload distance by some distance relative to load distance, to prevent chunks getting unloaded too early.
            _chunkUnloadRadiusSquared = (_chunkLoadRadius + 1) * (_chunkLoadRadius + 1);
            _chunkCenterOffset = new Vector2Int(Constants.CHUNK_SIZE_UNITS / 2, Constants.CHUNK_SIZE_UNITS / 2);
            
            // Initialize the material database before any chunks are loaded.
            _tileDatabase = _availableMaterials.CreateDatabase();
            _chunkGenerator.Initialize(_tileDatabase);
        }


        private void Start()
        {
            PrecomputeChunkLoadSpiral();
        }


        private void Update()
        {
            _playerChunkPosition = WorldToChunk(DrillController.Instance.transform.position);
            UnloadChunks();
            LoadChunks();
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void BreakTilesInRange(Span<uint> replacedTiles, Vector2 centerPosition, int radius)
        {
            GetAndSetTilesInRange(replacedTiles, centerPosition, _tileDatabase.AirTile, radius);
        }


        public void GetAndSetTilesInRange(Span<uint> replacedTiles, Vector2 centerPosition, in TileData newTile, int radius, bool circularRadius = true)
        {
            if (radius > Constants.CHUNK_SIZE_PIXELS / 2 - 1)
            {
                Debug.LogError("Radius is too large for the chunk size.");
                return;
            }
            
            int chunkCount = GetNeighbouringChunks(centerPosition, _chunkNeighbourhoodBuffer);
            
            // Now we can just set the tiles in all of the chunks.
            for (int i = 0; i < chunkCount; i++)
            {
                Chunk chunk = _chunkNeighbourhoodBuffer[i];
                if (chunk == null)
                    continue;
                
                Vector2Int chunkPosition = chunk.Position;
                Vector2 centerPositionRelativeUV = GetChunkTileUv(chunkPosition, centerPosition);
                
                UVToTileCoordinates(centerPositionRelativeUV, out int centerTileRelativeX, out int centerTileRelativeY);
                
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        int tilePosX = centerTileRelativeX + x;
                        int tilePosY = centerTileRelativeY + y;
                        
                        // Skip tiles outside of this chunk.
                        if (tilePosX < 0 || tilePosY < 0 || tilePosX >= Constants.CHUNK_SIZE_PIXELS || tilePosY >= Constants.CHUNK_SIZE_PIXELS)
                            continue;
                        
                        if (circularRadius && x * x + y * y > radius * radius)
                            continue;
                        
                        byte oldTile = chunk.GetAndSetTile(tilePosX, tilePosY, newTile);
                        
                        replacedTiles[oldTile] += 1;
                    }
                }
            }
        }
        
        
        public void SetTilesInRange(Vector2 centerPosition, in TileData newTile, int radius, bool circularRadius = true)
        {
            if (radius > Constants.CHUNK_SIZE_PIXELS / 2 - 1)
            {
                Debug.LogError("Radius is too large for the chunk size.");
                return;
            }
            
            int chunkCount = GetNeighbouringChunks(centerPosition, _chunkNeighbourhoodBuffer);
            
            // Now we can just set the tiles in all of the chunks.
            for (int i = 0; i < chunkCount; i++)
            {
                Chunk chunk = _chunkNeighbourhoodBuffer[i];
                if (chunk == null)
                    continue;
                
                Vector2Int chunkPosition = chunk.Position;
                Vector2 centerPositionRelativeUV = GetChunkTileUv(chunkPosition, centerPosition);
                
                UVToTileCoordinates(centerPositionRelativeUV, out int centerTileRelativeX, out int centerTileRelativeY);
                
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        int tilePosX = centerTileRelativeX + x;
                        int tilePosY = centerTileRelativeY + y;
                        
                        // Skip tiles outside of this chunk.
                        if (tilePosX < 0 || tilePosY < 0 || tilePosX >= Constants.CHUNK_SIZE_PIXELS || tilePosY >= Constants.CHUNK_SIZE_PIXELS)
                            continue;
                        
                        if (circularRadius && x * x + y * y > radius * radius)
                            continue;
                        
                        chunk.SetTile(tilePosX, tilePosY, newTile);
                    }
                }
            }
        }


        public void SetTile(Vector2 position, in TileData newTile)
        {
            Vector2Int chunkPosition = WorldToChunk(position);
            Vector2 tileUV = GetChunkTileUv(chunkPosition, position);

            UVToTileCoordinates(tileUV, out int tileX, out int tileY);
            
            SetTile(chunkPosition, tileX, tileY, newTile);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetTile(Vector2Int chunkPosition, int tileX, int tileY, in TileData newTile)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Constants.CHUNK_SIZE_PIXELS || tileY >= Constants.CHUNK_SIZE_PIXELS)
                return;
            if (_loadedChunksMap.TryGetValue(chunkPosition, out Chunk chunk))
            {
                chunk.SetTile(tileX, tileY, newTile);
            }
        }


        private int GetNeighbouringChunks(Vector2 position, IList<Chunk> buffer)
        {
            Vector2Int chunkPosition = WorldToChunk(position);
            int chunkCount = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int offset = new(x * Constants.CHUNK_SIZE_UNITS, y * Constants.CHUNK_SIZE_UNITS);
                    Vector2Int chunkPos = chunkPosition + offset;
                    if (_loadedChunksMap.TryGetValue(chunkPos, out Chunk chunk))
                    {
                        buffer[chunkCount] = chunk;
                        chunkCount++;
                    }
                }
            }

            return chunkCount;
        }


        private void UnloadChunks()
        {
            List<Vector2Int> chunksToUnload = new();
            foreach (Vector2Int chunk in _loadedChunks)
            {
                if (IsChunkInsideUnloadDistance(chunk))
                    continue;
                
                chunksToUnload.Add(chunk);
            }

            foreach (Vector2Int chunkPos in chunksToUnload)
            {
                Chunk chunk = _loadedChunksMap[chunkPos];
                _loadedChunksMap.Remove(chunkPos);
                _loadedChunks.Remove(chunkPos);
                Destroy(chunk.gameObject);
            }
        }


        private void LoadChunks()
        {
            foreach (Vector2Int spiralPos in _chunkLoadSpiral)
            {
                Vector2Int chunkPosition = _playerChunkPosition + spiralPos;
                
                if (_loadedChunks.Contains(chunkPosition))
                    continue;

                CreateChunk(chunkPosition);
            }
        }


        private void CreateChunk(Vector2Int position)
        {
            Chunk chunk = Instantiate(_chunkPrefab, (Vector2)position, Quaternion.identity, transform);
            chunk.Initialize(Constants.CHUNK_SIZE_PIXELS, Constants.TEXTURE_PPU, position);
            _loadedChunksMap.Add(position, chunk);
            _loadedChunks.Add(position);
            
            _chunkGenerator.QueueChunkGeneration(chunk);
        }
        
        
        private bool IsChunkInsideUnloadDistance(Vector2Int chunkPosition)
        {
            Vector2Int normalizedColumnPos = (chunkPosition - _playerChunkPosition) / Constants.CHUNK_SIZE_UNITS;
            return normalizedColumnPos.x * normalizedColumnPos.x + normalizedColumnPos.y * normalizedColumnPos.y <= _chunkUnloadRadiusSquared;
        }
    
    
        /// <summary>
        /// World position -> chunk position.
        /// Example: (36, 74, -5) -> (32, 64, -32)
        /// </summary>
        /// <returns>A new position that is relative to the chunk grid</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2Int WorldToChunk(Vector2 position)
        {
            return new Vector2Int(
                (int)Math.Floor(position.x) & ~Constants.CHUNK_SIZE_UNITS_BITMASK,
                (int)Math.Floor(position.y) & ~Constants.CHUNK_SIZE_UNITS_BITMASK
            );
        }


        /// <summary>
        /// World position -> chunk position.
        /// Example: (36, 74, -5) -> (32, 64, -32)
        /// </summary>
        /// <returns>A new position that is relative to the chunk grid</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2Int WorldToChunk(Vector2Int position)
        {
            return new Vector2Int(
                position.x & ~Constants.CHUNK_SIZE_UNITS_BITMASK,
                position.y & ~Constants.CHUNK_SIZE_UNITS_BITMASK
            );
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Vector2 GetChunkTileUv(Vector2Int chunkPosition, Vector2 tilePosition)
        {
            Vector2 localPosition = tilePosition - chunkPosition;
            Vector2 uv = localPosition / Constants.CHUNK_SIZE_UNITS;
            return uv;
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UVToTileCoordinates(Vector2 uv, out int x, out int y)
        {
            x = Mathf.FloorToInt(uv.x * Constants.CHUNK_SIZE_PIXELS);
            y = Mathf.FloorToInt(uv.y * Constants.CHUNK_SIZE_PIXELS);
        }
        
        
        private void PrecomputeChunkLoadSpiral()
        {
            int size = _chunkLoadRadius * 2 + 1;
            _chunkLoadSpiral = new List<Vector2Int>
            {
                new(0, 0)
            };

            foreach (Vector2Int pos in EnumerateSpiral(size * size - 1))
            {
                bool inRange = pos.x * pos.x + pos.y * pos.y <= _chunkLoadRadiusSquared;
                
                if (!inRange)
                    continue;
                Vector2Int position = pos * Constants.CHUNK_SIZE_UNITS;
                _chunkLoadSpiral.Add(position);
            }
        }
        
        
        private static IEnumerable<Vector2Int> EnumerateSpiral(int size)
        {
            int di = 1;
            int dj = 0;
            int segmentLength = 1;

            int i = 0;
            int j = 0;
            int segmentPassed = 0;
            for (int k = 0; k < size; ++k)
            {
                i += di;
                j += dj;
                ++segmentPassed;
                yield return new Vector2Int(i, j);

                if (segmentPassed != segmentLength)
                    continue;

                segmentPassed = 0;

                int buffer = di;
                di = -dj;
                dj = buffer;

                if (dj == 0)
                    ++segmentLength;
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (DrillController.Instance == null)
            {
                Gizmos.DrawWireSphere(Vector3.zero, _chunkLoadRadius * Constants.CHUNK_SIZE_PIXELS / (float)Constants.TEXTURE_PPU);
                return;
            }
            Vector2Int playerChunkPosition = WorldToChunk(DrillController.Instance.transform.position);
            Gizmos.DrawWireSphere((Vector2)(playerChunkPosition + _chunkCenterOffset), _chunkLoadRadius * Constants.CHUNK_SIZE_UNITS);
        }
    }
}