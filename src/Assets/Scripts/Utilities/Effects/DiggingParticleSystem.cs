﻿using UnityEngine;
using World.Chunks;

namespace Utilities.Effects
{
    /// <summary>
    /// Particle system that changes color based on the terrain color at its position.
    /// </summary>
    public class DiggingParticleSystem : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _particleSystem;
        
        [SerializeField]
        private Color _colorAddition = new Color(0.2f, 0.2f, 0.2f, 0f);

        private ParticleSystem.MainModule _coloringModule;


        private void Awake()
        {
            _coloringModule = _particleSystem.main;
        }


        private void Update()
        {
            if (ChunkManager.Instance == null)
                return;
            
            Color terrainColor = _colorAddition + ChunkManager.Instance.GetTerrainColorAt(transform.position);
            Color terrainColorAlpha = _colorAddition + new Color(terrainColor.r, terrainColor.g, terrainColor.b, 0f);
            _coloringModule.startColor = new ParticleSystem.MinMaxGradient(terrainColor, terrainColorAlpha);
        }
    }
}