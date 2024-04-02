using Entities.Drill;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using World.Stations;

namespace UI
{
    public class DepthDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;

        [SerializeField]
        private Image _depthImage;

        [SerializeField]
        private RectTransform _stationProgressRoot;

        [SerializeField]
        private Image _stationProgressImage;

        [SerializeField]
        private RectTransform _stationDirectionImageRoot;


        private void Awake()
        {
            ShowStationLocation(false);

            DrillController.Instance.FirstImpact += OnDrillFirstImpact;
            TradingStation.PlayerEnter += OnStationEntered;
            TradingStation.PlayerExit += OnStationExited;
            TradingStationManager.StationCreated += OnStationCreated;
            TradingStationManager.StationDeleted += OnStationDeleted;
        }


        private void OnDestroy()
        {
            if (DrillController.Instance != null)
                DrillController.Instance.FirstImpact -= OnDrillFirstImpact;
            TradingStation.PlayerEnter -= OnStationEntered;
            TradingStationManager.StationCreated -= OnStationCreated;
            TradingStationManager.StationDeleted -= OnStationDeleted;
        }


        private void OnDrillFirstImpact() => ShowStationProgress(true);
        private void OnStationCreated() => ShowStationDirection(true);
        private void OnStationEntered(TradingStation station) => ShowStationLocation(false);
        private void OnStationExited(TradingStation station) => ShowStationProgress(true);
        private void OnStationDeleted() => ShowStationDirection(false);


        private void Update()
        {
            const float stationInterval = Constants.STATION_DEPTH_INTERVAL;
            
            float playerDepth = -DrillController.Instance.transform.position.y;
            float depthProgress = playerDepth % stationInterval;
            float stationProgress = depthProgress / stationInterval;

            UpdateDepth(playerDepth);

            UpdateStationProgress(stationProgress);

            UpdateStationDirection();
        }


        private void UpdateDepth(float playerDepth)
        {
            // Rotate depth image.
            float depthImageRotation = playerDepth < 0 ? 0 : 180;
            _depthImage.rectTransform.localRotation = Quaternion.Euler(0, 0, depthImageRotation);

            // Display the depth.
            int depth = Mathf.RoundToInt(playerDepth);
            _text.text = $"{depth:n0}m";
        }


        private void ShowStationLocation(bool show)
        {
            ShowStationDirection(show);
            ShowStationProgress(show);
        }


        private void UpdateStationProgress(float stationProgress)
        {
            _stationProgressImage.fillAmount = Mathf.Clamp01(stationProgress);
        }


        private void UpdateStationDirection()
        {
            if (TradingStationManager.StationInstance == null)
                return;
            
            // Rotate the station direction image to point to the next station.
            Vector2 stationPos = TradingStationManager.StationInstance.transform.position;
            Vector2 playerPos = DrillController.Instance.transform.position;
            Vector2 direction = (stationPos - playerPos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            _stationDirectionImageRoot.localRotation = Quaternion.Euler(0, 0, angle);
        }


        private void ShowStationDirection(bool display) => _stationDirectionImageRoot.gameObject.SetActive(display);
        private void ShowStationProgress(bool display) => _stationProgressRoot.gameObject.SetActive(display);
    }
}