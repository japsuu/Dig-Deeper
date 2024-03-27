using UnityEngine;
using World.Chunks;

namespace World.Population
{
    public abstract class ChunkPopulator : MonoBehaviour
    {
        public void Populate(Chunk chunk, Vector2 position)
        {
            PopulateInternal(chunk, position);
        }


        protected abstract void PopulateInternal(Chunk chunk, Vector2 position);
    }
}