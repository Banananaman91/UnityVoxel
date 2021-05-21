using System;
using TerrainData;
using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    [Serializable]
    public struct NoiseInfo
    {
        [SerializeField] private float _noiseScale;
        [SerializeField] private float _heightScale;
        [SerializeField] private int _octaves;
        [SerializeField] private float _lacunarity;
        [SerializeField] private float _dimension;
        [SerializeField] private NoiseType _noiseType;

        public float NoiseScale => _noiseScale;
        public float HeightScale => _heightScale;
        public int Octaves => _octaves;
        public float Lacunarity => _lacunarity;
        public float Dimension => _dimension;
        public NoiseType NoiseType => _noiseType;
    }
}
