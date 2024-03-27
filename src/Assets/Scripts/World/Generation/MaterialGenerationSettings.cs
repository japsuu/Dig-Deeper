using Materials;
using UnityEngine;
using UnityEngine.Serialization;

namespace World.Generation
{
    /// <summary>
    /// Defines how a material should be generated.
    /// </summary>
    [CreateAssetMenu(menuName = "DigDeeper/Generation/Create MaterialGenerationSettings", fileName = "GenSet_", order = 0)]
    public class MaterialGenerationSettings : ScriptableObject
    {
        [SerializeField]
        private MaterialDefinition _material;

        [FormerlySerializedAs("_threshold1")]
        [SerializeField]
        [Tooltip("The threshold below which the material should be generated.")]
        [Range(0f, 1f)]
        private float _temperatureThreshold = 0.5f;

        [FormerlySerializedAs("_threshold2")]
        [SerializeField]
        [Tooltip("The threshold below which the material should be generated.")]
        [Range(0f, 1f)]
        private float _pressureThreshold = 0.5f;

        [FormerlySerializedAs("_threshold3")]
        [SerializeField]
        [Tooltip("The threshold below which the material should be generated.")]
        [Range(0f, 1f)]
        private float _tectonicsThreshold = 0.5f;
        
        
        public MaterialDefinition Material => _material;
        public float TemperatureThreshold => _temperatureThreshold;
        public float PressureThreshold => _pressureThreshold;
        public float TectonicsThreshold => _tectonicsThreshold;
    }
}