using UnityEngine;

namespace Materials
{
    /// <summary>
    /// Runtime representation of a material.
    /// When passing to methods, use the 'in' keyword to avoid copying the struct:
    /// https://devblogs.microsoft.com/premier-developer/the-in-modifier-and-the-readonly-structs-in-c/
    /// </summary>
    public readonly struct TileData
    {
        public readonly byte ID;        // 1 byte
        public readonly Color Color;    // 16 bytes
        public readonly byte Hardness;  // 1 byte
        public readonly byte Value;     // 1 byte
        
        // We cannot cache the definition from the constructor, as tiles initialized from an array are initialized with the default parameterless constructor.
        public MaterialDefinition Definition => TileDatabase.Instance.TileDefinitions[ID];


        public TileData(MaterialDefinition definition)
        {
            ID = definition.DynamicId;
            Color = definition.Color;
            Hardness = definition.Hardness;
            Value = definition.Value;
        }
    }
}