using TMPro;
using UnityEngine;
using World.Stations;

namespace UI
{
    public class TradingStationUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject _stationUIRoot;

        [SerializeField]
        private TMP_Text _stationNameText;

        [SerializeField]
        private TMP_Text _descriptionText;

        [SerializeField]
        private TMP_Text _infoText;
        
        private bool _isUiActive;
        private TradingStation _station;

        
        private void Awake()
        {
            EventManager.TradingStations.PlayerEnterStation += OnPlayerEnterStation;
            EventManager.TradingStations.PlayerExitStation += OnPlayerExitStation;
            
            ShowUI(false);
        }


        private void OnDestroy()
        {
            EventManager.TradingStations.PlayerEnterStation -= OnPlayerEnterStation;
            EventManager.TradingStations.PlayerExitStation -= OnPlayerExitStation;
        }


        private void Update()
        {
            if (!_isUiActive)
                return;

            if (!Input.GetKeyDown(KeyCode.E))
                return;
            
            _station.SellPlayerMaterials();
            RefreshUITexts(_station);
        }


        private void ShowUI(bool show)
        {
            _stationUIRoot.SetActive(show);
            _isUiActive = show;
        }


        private void OnPlayerEnterStation(TradingStation station)
        {
            _station = station;
            ShowUI(true);
            
            RefreshUITexts(station);
        }


        private void RefreshUITexts(TradingStation station)
        {
            _stationNameText.text = $"Trading Station \"{station.Name}\"";
            _descriptionText.text = station.GetDescriptionText();
            _infoText.text = station.GetInfoText();
        }


        private void OnPlayerExitStation(TradingStation station)
        {
            _station = null;
            ShowUI(false);
        }
    }
}