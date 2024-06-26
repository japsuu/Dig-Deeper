﻿
using WeightedRandomSelector.Interfaces;

namespace WeightedRandomSelector {
    /// <summary>
    /// Uses Binary Search for picking random items
    /// Good for large sized number of items
    /// </summary>
    /// <typeparam name="T">Type of items you wish this selector returns</typeparam>
    public class StaticRandomSelectorBinary<T> : IRandomSelector<T> {

        System.Random random;
        T[] items;
        float[] CDA;
        
        /// <summary>
        /// Constructor, used by StaticRandomSelectorBuilder
        /// Needs array of items and CDA (Cummulative Distribution Array). 
        /// </summary>
        /// <param name="items">Items of type T</param>
        /// <param name="CDA">Cummulative Distribution Array</param>
        /// <param name="seed">Seed for internal random generator</param>
        public StaticRandomSelectorBinary(T[] items, float[] CDA, int seed) {

            this.items = items;
            this.CDA = CDA;
            random = new System.Random(seed);
        }
        
        /// <summary>
        /// Selects random item based on their weights.
        /// Uses binary search for random selection.
        /// </summary>
        /// <returns>Returns item</returns>
        public T SelectRandomItem() {

            float randomValue = (float) random.NextDouble();
            
            return items[ CDA.SelectIndexBinarySearch(randomValue) ];
        }

        /// <summary>
        /// Selects random item based on their weights.
        /// Uses binary search for random selection.
        /// </summary>
        /// <param name="randomValue">Random value from your uniform generator</param>
        /// <returns>Returns item</returns>
        public T SelectRandomItem(float randomValue) {
        
            return items[ CDA.SelectIndexBinarySearch(randomValue) ];
        }
    }
}