using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace World
{
    public class Light2DFade : MonoBehaviour
    {
        [SerializeField]
        private Light2D _light;
        
        [SerializeField]
        private float _fadeLength = 1f;
        
        
        private void Start()
        {
            DOTween.To(() => _light.intensity, x => _light.intensity = x, 0f, _fadeLength).SetEase(Ease.Linear);
        }
    }
}