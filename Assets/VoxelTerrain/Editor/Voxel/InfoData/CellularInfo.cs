using UnityEngine;
using VoxelTerrain.Editor.Noise;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    [System.Serializable]
    public struct CellularInfo
    {
        [SerializeField] private FastNoiseLite.CellularDistanceFunction _cellularDistanceFunction;
        [SerializeField] private FastNoiseLite.CellularReturnType _cellularReturnType;
        [SerializeField] private float _cellularJitterModifier;

        public FastNoiseLite.CellularDistanceFunction CellularDistanceFunction => _cellularDistanceFunction;
        public FastNoiseLite.CellularReturnType CellularReturnType => _cellularReturnType;
        public float CellularJitterModifier => _cellularJitterModifier;
    }
}
