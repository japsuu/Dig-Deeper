using Materials;
using UnityEngine;

namespace World.Generation
{
    public class ChunkData
    {
        public readonly Vector2Int ChunkPosition;
        public readonly TileData[] Tiles;
            
            
        public ChunkData(Vector2Int chunkPosition)
        {
            ChunkPosition = chunkPosition;
            Tiles = new TileData[Constants.CHUNK_SIZE_PIXELS * Constants.CHUNK_SIZE_PIXELS];
        }
    }
}