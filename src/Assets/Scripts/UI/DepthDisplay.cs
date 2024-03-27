using System;
using Entities.Drill;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class DepthDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _text;

        [SerializeField]
        private Image _depthImage;

        [SerializeField]
        private Image _stationProgressImage;


        private void Update()
        {
            const float stationInterval = Constants.STATION_DEPTH_INTERVAL;
            
            float playerDepth = -DrillController.Instance.transform.position.y;
            float depthProgress = playerDepth % stationInterval;
            float stationProgress = depthProgress / stationInterval;

            float depthImageRotation = playerDepth < 0 ? 180 : 0;
            _depthImage.rectTransform.localRotation = Quaternion.Euler(0, 0, depthImageRotation);

            int depth = Mathf.RoundToInt(playerDepth);
            _text.text = $"{depth:n0}m";
            
            _stationProgressImage.fillAmount = Mathf.Clamp01(stationProgress);
        }
    }
}