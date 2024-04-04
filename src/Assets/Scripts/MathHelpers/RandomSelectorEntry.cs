using System;

namespace MathHelpers
{
    [Serializable]
    public class RandomSelectorEntry<T>
    {
        public T Object;
        public float Weight = 100;
    }
}