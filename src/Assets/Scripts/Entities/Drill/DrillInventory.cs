using System.Collections.Generic;
using Materials;

namespace Entities.Drill
{
    /// <summary>
    /// Keeps track of what tile materials the player has collected.
    /// </summary>
    public class DrillInventory
    {
        private readonly Dictionary<byte, uint> _materials = new();
        
        
        /// <summary>
        /// Adds the given amount of material to the inventory.
        /// </summary>
        public void AddMaterial(byte material, uint amount)
        {
            if (_materials.TryGetValue(material, out uint count))
                _materials[material] = count + amount;
            else
                _materials.Add(material, amount);
            EventManager.PlayerInventory.OnMaterialCountChanged(material, _materials[material]);
        }
        
        
        /// <summary>
        /// Removes all material of the given type.
        /// </summary>
        /// <returns>The amount of material removed.</returns>
        public uint RemoveAllMaterial(byte material)
        {
            if (!_materials.Remove(material, out uint count))
                return 0;
            
            EventManager.PlayerInventory.OnMaterialCountChanged(material, 0);
            return count;
        }


        public float GetTotalValue()
        {
            float totalValue = 0;
            foreach (KeyValuePair<byte, uint> material in _materials)
            {
                totalValue += material.Value * TileDatabase.Instance.TileDefinitions[material.Key].Value;
            }
            return totalValue;
        }


        public void Clear()
        {
            _materials.Clear();
        }
    }
}