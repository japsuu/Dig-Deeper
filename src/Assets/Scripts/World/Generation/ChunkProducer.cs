using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using World.Chunks;

namespace World.Generation
{
    public abstract class ChunkProducer
    {
        protected readonly ChunkGenerator Generator;
        protected readonly MonoBehaviour MonoBehaviourHandle;


        protected ChunkProducer(ChunkGenerator generator, MonoBehaviour monoBehaviourHandle)
        {
            Generator = generator;
            MonoBehaviourHandle = monoBehaviourHandle;
        }


        /// <summary>
        /// Adds a chunk to the producer internal queue.
        /// Called from the main thread.
        /// </summary>
        /// <param name="chunk">The chunk to add to the queue.</param>
        public abstract void Post(Chunk chunk);

        /// <summary>
        /// Called from the main thread every frame.
        /// </summary>
        public abstract void Consume();
    }
    
    public class MultithreadedChunkProducer : ChunkProducer
    {
        private const int MAX_DEGREE_OF_PARALLELISM = 8;
        private const int MAX_CHUNKS_CONSUMED_PER_FRAME = 3;
        
        private readonly Queue<Chunk> _chunksToGenerate = new();
        private readonly ConcurrentQueue<ChunkData> _generatedChunks = new();


        public MultithreadedChunkProducer(ChunkGenerator generator, MonoBehaviour monoBehaviourHandle) : base(generator, monoBehaviourHandle)
        {
        }

        
        public override void Post(Chunk chunk)
        {
            _chunksToGenerate.Enqueue(chunk);
        }


        public override void Consume()
        {
            int enqueueCount = 0;
            while (enqueueCount < MAX_DEGREE_OF_PARALLELISM && _chunksToGenerate.TryDequeue(out Chunk data))
            {
                // Queue to thread pool.
                ThreadPool.QueueUserWorkItem(_ => ProcessChunk(data));
                
                enqueueCount++;
            }
            
            int processedCount = 0;
            while (processedCount < MAX_CHUNKS_CONSUMED_PER_FRAME && _generatedChunks.TryDequeue(out ChunkData data))
            {
                ChunkManager.Instance.AddChunkData(data.ChunkPosition, data.Tiles, data.ChunkPosition.y < Generator.PopulationLevel);
                
                processedCount++;
            }
        }


        private void ProcessChunk(Chunk chunk)
        {
            if (Generator.TryGenerateChunk(chunk, out ChunkData data))
                _generatedChunks.Enqueue(data);
        }
    }
    
    public class WebGLChunkProducer : ChunkProducer
    {
        private const int MAX_DEGREE_OF_CONCURRENCY = 1;
        
        private readonly Queue<Chunk> _chunksToGenerate = new();


        public WebGLChunkProducer(ChunkGenerator generator, MonoBehaviour monoBehaviourHandle) : base(generator, monoBehaviourHandle)
        {
        }
        
        
        public override void Post(Chunk chunk)
        {
            _chunksToGenerate.Enqueue(chunk);
        }


        public override void Consume()
        {
            int enqueueCount = 0;
            while (enqueueCount < MAX_DEGREE_OF_CONCURRENCY && _chunksToGenerate.TryDequeue(out Chunk chunk))
            {
                // Start a generation coroutine.
                MonoBehaviourHandle.StartCoroutine(Generator.GenerateChunkCoroutine(chunk));
                
                enqueueCount++;
            }
        }
    }
}