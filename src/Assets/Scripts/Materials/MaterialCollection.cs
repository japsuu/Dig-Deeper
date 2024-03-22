using System.Collections.Generic;
using UnityEngine;

namespace Materials
{
    [CreateAssetMenu(menuName = "DigDeeper/Materials/Create MaterialCollection", fileName = "MaterialCollection", order = 0)]
    public class MaterialCollection : ScriptableObject
    {
        [SerializeField]
        private MaterialDefinition _airMaterial;
        
        [SerializeField]
        private List<MaterialDefinition> _materials;
        
        
        public TileDatabase CreateDatabase()
        {
            Debug.Assert(_airMaterial != null, "Air material is not set.");
            Debug.Assert(_materials != null, "Materials list is not set.");
            List<MaterialDefinition> materials = new() { _airMaterial };
            materials.AddRange(_materials);
            return TileDatabase.InitializeSingleton(materials);
        }
    }
}