using System;
using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    [Serializable]
    public struct NoiseInfo
    {
        [SerializeField] private float _noiseScale;
        [SerializeField] private float _heightScale;
        [SerializeField] private float _altitudeScale;
        [SerializeField] private float _moistureScale;
        [SerializeField] private int _octaves;
        [SerializeField] private float _lacunarity;

        public float NoiseScale => _noiseScale;
        public float HeightScale => _heightScale;
        public float AltitudeScale => _altitudeScale;
        public float MoistureScale => _moistureScale;
        public int Octaves => _octaves;
        public float Lacunarity => _lacunarity;
    }
}
