using System.Collections.Generic;
using WeightedRandomSelector;
using WeightedRandomSelector.Interfaces;

namespace MathHelpers
{
    public static class RandomSelectorBuilder
    {
        public static IRandomSelector<T> Build<T>(List<RandomSelectorEntry<T>> entries)
        {
            DynamicRandomSelector<T> selector = new();
            
            foreach (RandomSelectorEntry<T> entry in entries)
            {
                T o = entry.Object;
                if (o != null)
                    selector.Add(o, entry.Weight);
            }

            return selector.Build();
        }
    }
}