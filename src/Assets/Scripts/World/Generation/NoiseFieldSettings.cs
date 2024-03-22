using System;
using Thirdparty;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World.Generation
{
    [Serializable]
    public class NoiseFieldSettings
    {
        [Header("General")]
        public FastNoiseLite.NoiseType NoiseType = FastNoiseLite.NoiseType.OpenSimplex2;
        public float Frequency = 0.01f;
        
        [Header("Fractal")]
        public FastNoiseLite.FractalType FractalType = FastNoiseLite.FractalType.None;
        public int Octaves = 3;
        public float Lacunarity = 2f;
        public float Gain = 0.5f;
        public float WeightedStrength = 0f;
        
        [Header("Cellular")]
        public FastNoiseLite.CellularDistanceFunction CellularDistanceFunction = FastNoiseLite.CellularDistanceFunction.EuclideanSq;
        public FastNoiseLite.CellularReturnType CellularReturnType = FastNoiseLite.CellularReturnType.Distance;
        public float CellularJitter = 1f;
        
        
        public FastNoiseLite GetNoise()
        {
            FastNoiseLite noise = new(Random.Range(0, int.MaxValue));
            noise.SetNoiseType(NoiseType);
            noise.SetFrequency(Frequency);
            
            noise.SetFractalType(FractalType);
            noise.SetFractalOctaves(Octaves);
            noise.SetFractalLacunarity(Lacunarity);
            noise.SetFractalGain(Gain);
            noise.SetFractalWeightedStrength(WeightedStrength);
            
            noise.SetCellularDistanceFunction(CellularDistanceFunction);
            noise.SetCellularReturnType(CellularReturnType);
            noise.SetCellularJitter(CellularJitter);
            
            return noise;
        }
    }
}