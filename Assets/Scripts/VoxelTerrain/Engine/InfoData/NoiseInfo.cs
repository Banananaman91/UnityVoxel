using System;
using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    [Serializable]
    public struct NoiseInfo
    {
        [SerializeField] private float _noiseScale;
        [SerializeField] private int _octaves;
        [SerializeField] private float _lacunarity;
        [SerializeField] private float _dimension;

        public float NoiseScale => _noiseScale;
        public int Octaves => _octaves;
        public float Lacunarity => _lacunarity;
        public float Dimension => _dimension;
    }
}
