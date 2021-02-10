using UnityEngine;
using VoxelTerrain.Editor.Noise;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    [System.Serializable]
    public struct FractalInfo
    {
        [SerializeField] private FastNoiseLite.FractalType _fractalType;
        [SerializeField] private int _octaves;
        [SerializeField] private float _lacunarity;
        [SerializeField] private float _gain;
        [SerializeField] private float _weightedStrength;
        [SerializeField] private float _pingPongStength;

        public FastNoiseLite.FractalType FractalType => _fractalType;
        public int Octaves => _octaves;
        public float Lacunarity => _lacunarity;
        public float Gain => _gain;
        public float WeightedStrength => _weightedStrength;
        public float PingPongStrength => _pingPongStength;
    }
}
