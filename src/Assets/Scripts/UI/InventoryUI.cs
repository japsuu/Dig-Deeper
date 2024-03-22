using System.Collections.Generic;
using Drill;
using Materials;
using TMPro;
using UnityEngine;

namespace UI
{
    public class InventoryUI : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _inventoryEntryPrefab;
        
        private readonly Dictionary<byte, TMP_Text> _inventoryEntries = new();


        private void Start()
        {
            for (int i = 0; i < TileDatabase.Instance.RegisteredTileCount; i++)
            {
                TileData tileData = TileDatabase.Instance.TileData[i];
                
                TMP_Text entry = Instantiate(_inventoryEntryPrefab, transform);
                _inventoryEntries.Add(tileData.ID, entry);
                
                UpdateText(entry, tileData, 0);
            }
            
            DrillController.Instance.Inventory.MaterialCountChanged += OnMaterialCountChanged;
        }


        private void OnMaterialCountChanged(byte id, uint count)
        {
            UpdateText(_inventoryEntries[id], TileDatabase.Instance.TileData[id], count);
        }
        
        
        private void UpdateText(TMP_Text text, TileData tile, uint count)
        {
            text.text = $"{tile.Definition.Name}: {count}";
        }
    }
}