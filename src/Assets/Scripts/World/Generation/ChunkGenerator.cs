using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Materials;
using Thirdparty;
using World.Chunks;

namespace World.Generation
{
    public class ChunkGenerator
    {
        // Optimization constants.
        // A chunk is divided into "bricks" to optimize noise sampling by taking less samples.
        private const int NOISE_SAMPLE_BRICK_SIZE = 4;
        private const int BRICK_COUNT_X = Constants.CHUNK_SIZE_PIXELS / NOISE_SAMPLE_BRICK_SIZE;
        private const int BRICK_COUNT_Y = Constants.CHUNK_SIZE_PIXELS / NOISE_SAMPLE_BRICK_SIZE;
        
        private readonly TileDatabase _tileDatabase;
        private readonly FastNoiseLite _caveNoise;
        private readonly FastNoiseLite _temperatureNoise;
        private readonly FastNoiseLite _pressureNoise;
        private readonly FastNoiseLite _tectonicsNoise;
        private readonly MaterialDefinition _baseGroundMaterial;
        private readonly List<MaterialGenerationSettings> _materialGenerationSettings;
        private readonly int _groundLevel;
        public readonly int PopulationLevel;
        private readonly float _caveFactor;


        public ChunkGenerator(
            TileDatabase tileDatabase,
            FastNoiseLite caveNoise,
            FastNoiseLite temperatureNoise,
            FastNoiseLite pressureNoise,
            FastNoiseLite tectonicsNoise,
            MaterialDefinition baseGroundMaterial,
            List<MaterialGenerationSettings> materialGenerationSettings,
            int groundLevel,
            int populationLevel,
            float caveFactor)
        {
            _tileDatabase = tileDatabase;
            _caveNoise = caveNoise;
            _temperatureNoise = temperatureNoise;
            _pressureNoise = pressureNoise;
            _tectonicsNoise = tectonicsNoise;
            _baseGroundMaterial = baseGroundMaterial;
            _materialGenerationSettings = materialGenerationSettings;
            _groundLevel = groundLevel;
            PopulationLevel = populationLevel;
            _caveFactor = caveFactor;
        }


        /// <summary>
        /// Blocking method that generates the chunk data and returns it.
        /// </summary>
        /// <param name="chunk">The chunk to generate.</param>
        /// <param name="data">The generated chunk data.</param>
        /// <returns>If the chunk was successfully generated.</returns>
        public bool TryGenerateChunk(Chunk chunk, out ChunkData data)
        {
            data = new ChunkData(chunk.Position);
            if (chunk == null)
                return false; // May happen if the chunk was unloaded before generation.
            
            int chunkXPixels = chunk.Position.x * Constants.TEXTURE_PPU;
            int chunkYPixels = chunk.Position.y * Constants.TEXTURE_PPU;

            if (chunk.Position.y > _groundLevel)
                return true; // Above ground level, no need to generate anything.
            
            //TODO: Generate surface with varying height.

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

                            TileData tile = GenerateTerrain(density, temperature, pressure, tectonics);
                            data.Tiles[Chunk.GetArrayIndex(chunkRelativeX, chunkRelativeY)] = tile;
                        }
                    }
                }
            }
            
            return true;
        }
        
        
        /// <summary>
        /// Coroutine that generates the chunk data and yields until it's done.
        /// </summary>
        /// <param name="chunk"></param>
        /// <returns></returns>
        public IEnumerator GenerateChunkCoroutine(Chunk chunk)
        {
            if (chunk == null)
                yield break; // May happen if the chunk was unloaded before generation.
            
            int chunkXPixels = chunk.Position.x * Constants.TEXTURE_PPU;
            int chunkYPixels = chunk.Position.y * Constants.TEXTURE_PPU;

            ChunkData data = new(chunk.Position);
            
            if (chunk.Position.y > _groundLevel)
            {
                // Above ground level, no need to generate anything.
                ChunkManager.Instance.AddChunkData(data.ChunkPosition, data.Tiles, data.ChunkPosition.y < PopulationLevel);
                yield break;
            }
            
            //TODO: Generate surface with varying height.

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

                            TileData tile = GenerateTerrain(density, temperature, pressure, tectonics);
                            data.Tiles[Chunk.GetArrayIndex(chunkRelativeX, chunkRelativeY)] = tile;
                        }
                    }
                }
                yield return null;
            }
            
            ChunkManager.Instance.AddChunkData(data.ChunkPosition, data.Tiles, data.ChunkPosition.y < PopulationLevel);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TileData GenerateTerrain(float density, float temperature, float pressure, float tectonics)
        {
            // Carve caves.
            if (density < _caveFactor)
                return _tileDatabase.AirTile;

            byte tileId = GetTileAt(temperature, pressure, tectonics);
            return _tileDatabase.TileData[tileId];
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private byte GetTileAt(float temp, float pressure, float tectonics)
        {
            foreach (MaterialGenerationSettings s in _materialGenerationSettings)
            {
                if (s.TemperatureThreshold < temp)
                    continue;

                if (s.PressureThreshold < pressure)
                    continue;

                if (s.TectonicsThreshold < tectonics)
                    continue;

                return s.Material.DynamicId;
            }

            return _baseGroundMaterial.DynamicId;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float InterpolateNoise(float c00, float c10, float c01, float c11, float x, float y)
        {
            // Simple bi-linear interpolation.
            float c0 = c00 * (1 - x) + c10 * x;
            float c1 = c01 * (1 - x) + c11 * x;

            return c0 * (1 - y) + c1 * y;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float GetNoiseValue(FastNoiseLite fnl, float x, float y) => (fnl.GetNoise(x, y) + 1) / 2;
    }
}