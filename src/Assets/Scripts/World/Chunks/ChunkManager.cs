using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Entities.Drill;
using Materials;
using Singletons;
using UnityEngine;
using World.Generation;

namespace World.Chunks
{
    /// <summary>
    /// Manages chunk loading/unloading.
    /// TODO: Refactor this gargantuan class into smaller parts.
    /// TODO: Handle origin shifting if the player digs too deep, or just seamlessly teleport them to the top of the map.
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
        private ChunkGenerationManager _chunkGenerationManager;
        private Vector2Int _playerChunkPosition;
        private List<Vector2Int> _chunkLoadSpiral; // Precomputed spiral of chunk positions to load.
        private TileDatabase _tileDatabase;
        private readonly Chunk[] _chunkNeighbourhoodBuffer = new Chunk[9];
        private readonly HashSet<Vector2Int> _loadedChunks = new();
        private readonly Dictionary<Vector2Int, Chunk> _loadedChunksMap = new();

        public int RegisteredTileCount => _tileDatabase.RegisteredTileCount;


        private void Awake()
        {
            _chunkGenerationManager = GetComponent<ChunkGenerationManager>();
            _chunkLoadRadiusSquared = _chunkLoadRadius * _chunkLoadRadius;

            // Extend the unload distance by some distance relative to load distance, to prevent chunks getting unloaded too early.
            _chunkUnloadRadiusSquared = (_chunkLoadRadius + 1) * (_chunkLoadRadius + 1);
            _chunkCenterOffset = new Vector2Int(Constants.CHUNK_SIZE_UNITS / 2, Constants.CHUNK_SIZE_UNITS / 2);

            // Initialize the material database before any chunks are loaded.
            _tileDatabase = _availableMaterials.CreateDatabase();
            _chunkGenerationManager.Initialize(_tileDatabase);
        }


        private void Start()
        {
            PrecomputeChunkLoadSpiral();
        }


        private void Update()
        {
            _playerChunkPosition = WorldToChunk(DrillStateMachine.Instance.transform.position);
            UnloadChunks();
            LoadChunks();
        }


        public void AddChunkData(Vector2Int chunkPosition, TileData[] data, bool canPopulate)
        {
            if (!_loadedChunksMap.TryGetValue(chunkPosition, out Chunk chunk))
                return;

            chunk.OnGenerated(data, canPopulate);
        }


        public bool ContainsNonAirTilesInRange(Vector2 centerPosition, int radius)
        {
            int chunkCount = GetNeighbouringChunks(centerPosition, _chunkNeighbourhoodBuffer);

            for (int i = 0; i < chunkCount; i++)
            {
                Chunk chunk = _chunkNeighbourhoodBuffer[i];
                if (chunk == null)
                    continue;

                Vector2Int chunkPosition = chunk.Position;
                Vector2 centerPositionRelativeUV = GetChunkTileUv(chunkPosition, centerPosition);

                WorldToTile(centerPositionRelativeUV, out int centerTileRelativeX, out int centerTileRelativeY);

                for (int x = -radius; x <= radius; x++)
                for (int y = -radius; y <= radius; y++)
                {
                    int tilePosX = centerTileRelativeX + x;
                    int tilePosY = centerTileRelativeY + y;

                    // Skip tiles outside of this chunk.
                    if (tilePosX < 0 || tilePosY < 0 || tilePosX >= Constants.CHUNK_SIZE_PIXELS || tilePosY >= Constants.CHUNK_SIZE_PIXELS)
                        continue;

                    if (chunk.GetTile(tilePosX, tilePosY) != 0)
                        return true;
                }
            }

            return false;
        }


        public byte GetTerrainHardnessAt(Vector2 position)
        {
            if (!GetChunkAt(position, out Chunk chunk))
                return 0;

            Vector2Int chunkPosition = chunk.Position;
            Vector2 centerPositionRelativeUV = GetChunkTileUv(chunkPosition, position);

            WorldToTile(centerPositionRelativeUV, out int posX, out int posY);

            return chunk.GetHardness(posX, posY);
        }


        public Color GetTerrainColorAt(Vector2 position)
        {
            if (!GetChunkAt(position, out Chunk chunk))
                return Color.clear;

            Vector2Int chunkPosition = chunk.Position;
            Vector2 centerPositionRelativeUV = GetChunkTileUv(chunkPosition, position);

            WorldToTile(centerPositionRelativeUV, out int posX, out int posY);

            byte tileId = chunk.GetTile(posX, posY);
            return _tileDatabase.TileData[tileId].Color;
        }


        #region CHUNK MODIFICATION

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAndSetTilesInRange(Span<uint> replacedTiles, Vector2 centerPosition, int radius)
        {
            GetAndSetTilesInRange(replacedTiles, centerPosition, radius, _tileDatabase.AirTile);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAndSetTilesInRange(Span<uint> replacedTiles, Vector2 centerPosition, int radius, in TileData newTile, bool circularRadius = true)
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

                WorldToTile(centerPositionRelativeUV, out int centerTileRelativeX, out int centerTileRelativeY);

                for (int x = -radius; x <= radius; x++)
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

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTilesInRange(Vector2 centerPosition, int radius, in TileData newTile, bool circularRadius = true)
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

                WorldToTile(centerPositionRelativeUV, out int centerTileRelativeX, out int centerTileRelativeY);

                for (int x = -radius; x <= radius; x++)
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
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTilesInRange(Vector2 centerPosition, int radius, bool circularRadius = true)
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

                WorldToTile(centerPositionRelativeUV, out int centerTileRelativeX, out int centerTileRelativeY);

                for (int x = -radius; x <= radius; x++)
                for (int y = -radius; y <= radius; y++)
                {
                    int tilePosX = centerTileRelativeX + x;
                    int tilePosY = centerTileRelativeY + y;

                    // Skip tiles outside of this chunk.
                    if (tilePosX < 0 || tilePosY < 0 || tilePosX >= Constants.CHUNK_SIZE_PIXELS || tilePosY >= Constants.CHUNK_SIZE_PIXELS)
                        continue;

                    if (circularRadius && x * x + y * y > radius * radius)
                        continue;

                    chunk.SetTile(tilePosX, tilePosY, _tileDatabase.AirTile);
                }
            }
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAndSetTilesInLine(Span<uint> replacedTiles, Vector2 startPosition, Vector2 endPosition, int radius)
        {
            GetAndSetTilesInLine(replacedTiles, startPosition, endPosition, radius, _tileDatabase.AirTile);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetAndSetTilesInLine(Span<uint> replacedTiles, Vector2 startPosition, Vector2 endPosition, int radius, in TileData tile)
        {
            // Convert the provided world positions to tile (pixel) positions.
            WorldToTile(startPosition, out int startX, out int startY);
            WorldToTile(endPosition, out int endX, out int endY);

            // Loop through all the points in pixel-space between the start and end positions.
            foreach (Vector2Int point in BresenhamLine2(startX, startY, endX, endY))
            {
                // Convert the pixel-space point back to world-space.
                Vector2 worldPosition = new Vector2(point.x / (float)Constants.CHUNK_SIZE_PIXELS, point.y / (float)Constants.CHUNK_SIZE_PIXELS);
                
                GetAndSetTilesInRange(replacedTiles, worldPosition, radius, tile);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTilesInLine(Vector2 startPosition, Vector2 endPosition, int width)
        {//TODO: Fixme
            Vector2Int startChunkPosition = WorldToChunk(startPosition);
            Vector2Int endChunkPosition = WorldToChunk(endPosition);

            foreach (Vector2Int point in BresenhamLine2(startChunkPosition.x, startChunkPosition.y, endChunkPosition.x, endChunkPosition.y))
                for (int xOffset = -width; xOffset <= width; xOffset++)
                for (int yOffset = -width; yOffset <= width; yOffset++)
                {
                    Vector2Int offsetPoint = point + new Vector2Int(xOffset, yOffset);
                    SetTile(offsetPoint, _tileDatabase.AirTile);
                }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTilesInLine(Vector2 startPosition, Vector2 endPosition, int width, TileData tile)
        {
            Vector2Int startChunkPosition = WorldToChunk(startPosition);
            Vector2Int endChunkPosition = WorldToChunk(endPosition);

            foreach (Vector2Int point in BresenhamLine2(startChunkPosition.x, startChunkPosition.y, endChunkPosition.x, endChunkPosition.y))
                for (int xOffset = -width; xOffset <= width; xOffset++)
                for (int yOffset = -width; yOffset <= width; yOffset++)
                {
                    Vector2Int offsetPoint = point + new Vector2Int(xOffset, yOffset);
                    SetTile(offsetPoint, tile);
                }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetTile(Vector2 position, in TileData newTile)
        {
            Vector2Int chunkPosition = WorldToChunk(position);
            Vector2 tileUV = GetChunkTileUv(chunkPosition, position);

            WorldToTile(tileUV, out int tileX, out int tileY);

            SetTileInChunk(chunkPosition, tileX, tileY, newTile);
        }

        #endregion


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetTileInChunk(Vector2Int chunkPosition, int tileX, int tileY, in TileData newTile)
        {
            if (tileX < 0 || tileY < 0 || tileX >= Constants.CHUNK_SIZE_PIXELS || tileY >= Constants.CHUNK_SIZE_PIXELS)
                return;
            if (_loadedChunksMap.TryGetValue(chunkPosition, out Chunk chunk))
                chunk.SetTile(tileX, tileY, newTile);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetNeighbouringChunks(Vector2 position, IList<Chunk> buffer)
        {
            Vector2Int chunkPosition = WorldToChunk(position);
            int chunkCount = 0;
            for (int x = -1; x <= 1; x++)
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

            return chunkCount;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool GetChunkAt(Vector2 position, out Chunk chunk)
        {
            Vector2Int chunkPosition = WorldToChunk(position);
            return _loadedChunksMap.TryGetValue(chunkPosition, out chunk);
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
            chunk.Initialize(position);
            _loadedChunksMap.Add(position, chunk);
            _loadedChunks.Add(position);

            _chunkGenerationManager.QueueChunkGeneration(chunk);
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
        private static Vector2Int WorldToChunk(Vector2 position) =>
            new(
                (int)Math.Floor(position.x) & ~Constants.CHUNK_SIZE_UNITS_BITMASK,
                (int)Math.Floor(position.y) & ~Constants.CHUNK_SIZE_UNITS_BITMASK
            );

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector2 GetChunkTileUv(Vector2Int chunkPosition, Vector2 tilePosition)
        {
            Vector2 localPosition = tilePosition - chunkPosition;
            Vector2 uv = localPosition / Constants.CHUNK_SIZE_UNITS;
            return uv;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void WorldToTile(Vector2 uv, out int x, out int y)
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


        // Faster, but less accurate Bresenham's Line Algorithm.
        // Modified from: http://ericw.ca/notes/bresenhams-line-algorithm-in-csharp.html
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<Vector2Int> BresenhamLine2(int x1, int y1, int x2, int y2)
        {
            bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);
            if (steep)
            {
                int t = x1;
                x1 = y1;
                y1 = t;
                t = x2;
                x2 = y2;
                y2 = t;
            }

            if (x1 > x2)
            {
                int t = x1;
                x1 = x2;
                x2 = t;
                t = y1;
                y1 = y2;
                y2 = t;
            }

            int dx = x2 - x1;
            int dy = Math.Abs(y2 - y1);
            int error = dx / 2;
            int yStep = y1 < y2 ? 1 : -1;
            int y = y1;
            for (int x = x1; x <= x2; x++)
            {
                yield return new Vector2Int(steep ? y : x, steep ? x : y);
                error -= dy;
                if (error >= 0)
                    continue;
                y += yStep;
                error += dx;
            }
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (DrillStateMachine.Instance == null)
            {
                Gizmos.DrawWireSphere(Vector3.zero, _chunkLoadRadius * Constants.CHUNK_SIZE_PIXELS / (float)Constants.TEXTURE_PPU);
                return;
            }

            Vector2Int playerChunkPosition = WorldToChunk(DrillStateMachine.Instance.transform.position);
            Gizmos.DrawWireSphere((Vector2)(playerChunkPosition + _chunkCenterOffset), _chunkLoadRadius * Constants.CHUNK_SIZE_UNITS);
        }
    }
}