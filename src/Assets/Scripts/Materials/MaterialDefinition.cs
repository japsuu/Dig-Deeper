using UnityEngine;

namespace Materials
{
    /// <summary>
    /// Defines the base properties of a tile material.
    /// Is an editor-friendly representation of a material.
    /// Converted to TileData at runtime.
    /// </summary>
    [CreateAssetMenu(menuName = "DigDeeper/Materials/Create MaterialDefinition", fileName = "Material_", order = 0)]
    public class MaterialDefinition : ScriptableObject
    {
        [SerializeField]
        [Tooltip("Name of the material.")]
        private string _name = "unknown";

        [SerializeField]
        [Tooltip("Color of the material.")]
        private Color _color = Color.white;

        [SerializeField]
        [Tooltip("How hard the material is. 0 = softest, 255 = hardest. Influences the speed of the drill when this material is hit.")]
        private byte _hardness = 0;

        [SerializeField]
        [Tooltip("Value in coins, when sold.")]
        private byte _value = 1;
        
        
        public byte DynamicId { get; private set; }
        public string Name => _name;
        public Color Color => _color;
        public byte Hardness => _hardness;
        public byte Value => _value;
        
        
        public void AssignDynamicId(byte id)
        {
            DynamicId = id;
        }
    }
}