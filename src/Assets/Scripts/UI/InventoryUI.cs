using System.Collections.Generic;
using Entities.Drill;
using Materials;
using TMPro;
using UnityEngine;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _inventoryEntryPrefab;

        [SerializeField]
        private List<MaterialDefinition> _ignoredMaterials;
        
        private readonly Dictionary<byte, TMP_Text> _inventoryEntries = new();


        private void Start()
        {
            for (int i = 0; i < TileDatabase.Instance.RegisteredTileCount; i++)
            {
                TileData tileData = TileDatabase.Instance.TileData[i];
                
                if (_ignoredMaterials.Contains(tileData.Definition))
                    continue;
                
                TMP_Text entry = Instantiate(_inventoryEntryPrefab, transform);
                _inventoryEntries.Add(tileData.ID, entry);
                
                UpdateText(entry, tileData, 0);
            }
            
            DrillController.Instance.Inventory.MaterialCountChanged += OnMaterialCountChanged;
        }


        private void OnMaterialCountChanged(byte id, uint count)
        {
            if (!_inventoryEntries.TryGetValue(id, out TMP_Text tmpText))
                return;
            
            UpdateText(tmpText, TileDatabase.Instance.TileData[id], count);
        }
        
        
        private void UpdateText(TMP_Text text, TileData tile, uint count)
        {
            text.text = $"{tile.Definition.Name}: {count}";
        }
    }
}