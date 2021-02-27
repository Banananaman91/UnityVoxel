using UnityEngine;
using VoxelTerrain.Editor.Noise;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    public class CellularInfo : MonoBehaviour
    {
        [SerializeField] private FastNoiseLite.CellularDistanceFunction _cellularDistanceFunction;
        [SerializeField] private FastNoiseLite.CellularReturnType _cellularReturnType;
        [SerializeField] private float _cellularJitterModifier;

        public FastNoiseLite.CellularDistanceFunction CellularDistanceFunction => _cellularDistanceFunction;
        public FastNoiseLite.CellularReturnType CellularReturnType => _cellularReturnType;
        public float CellularJitterModifier => _cellularJitterModifier;
    }
}
