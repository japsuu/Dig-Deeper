using System;
using System.Collections.Generic;

namespace Materials
{
    public class TileDatabase
    {
        public static TileDatabase Instance { get; private set; }
        
        /// <summary>
        /// The data of each material, indexed by the material's dynamic ID.
        /// </summary>
        public readonly TileData[] TileData;
        
        public readonly MaterialDefinition[] TileDefinitions;
        
        /// <summary>
        /// The number of materials registered in the database.
        /// </summary>
        public readonly int RegisteredTileCount;
        
        public TileData AirTile => TileData[0];
        
        
        public static TileDatabase InitializeSingleton(List<MaterialDefinition> materials)
        {
            if (Instance != null)
                throw new InvalidOperationException("TileDatabase has already been initialized.");
            Instance = new TileDatabase(materials);
            return Instance;
        }


        private TileDatabase(List<MaterialDefinition> materials)
        {
            RegisteredTileCount = materials.Count;
            TileData = new TileData[RegisteredTileCount];
            TileDefinitions = new MaterialDefinition[RegisteredTileCount];
            
            RegisterAndDistributeIds(materials);
        }


        private void RegisterAndDistributeIds(List<MaterialDefinition> materials)
        {
            if (materials.Count > byte.MaxValue)
                throw new InvalidOperationException("NotSupported: Material count exceeds maximum byte value.");
            
            for (byte i = 0; i < materials.Count; i++)
            {
                MaterialDefinition material = materials[i];
                material.AssignDynamicId(i);
                TileData[i] = new TileData(material);
                TileDefinitions[i] = material;
            }
        }
    }
}