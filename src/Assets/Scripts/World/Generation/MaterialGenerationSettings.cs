using Materials;
using UnityEngine;

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

        [SerializeField]
        [Tooltip("The threshold below which the material should be generated.")]
        [Range(0f, 1f)]
        private float _threshold1 = 0.5f;

        [SerializeField]
        [Tooltip("The threshold below which the material should be generated.")]
        [Range(0f, 1f)]
        private float _threshold2 = 0.5f;

        [SerializeField]
        [Tooltip("The threshold below which the material should be generated.")]
        [Range(0f, 1f)]
        private float _threshold3 = 0.5f;
        
        
        public MaterialDefinition Material => _material;
        public float Threshold1 => _threshold1;
        public float Threshold2 => _threshold2;
        public float Threshold3 => _threshold3;
    }
}