using System;
using TerrainData;
using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    [Serializable]
    public struct NoiseInfo
    {
        public int ThreeDimensional;
        public float NoiseScale;
        public float HeightScale;
        public int Octaves;
        public float Lacunarity;
        public float Dimension;
        public int NoiseType;
    }
}
