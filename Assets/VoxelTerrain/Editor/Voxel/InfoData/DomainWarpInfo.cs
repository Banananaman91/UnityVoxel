using UnityEngine;
using VoxelTerrain.Editor.Noise;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    public class DomainWarpInfo : MonoBehaviour
    {
        [SerializeField] private FastNoiseLite.DomainWarpType _domainWarpType;
        [SerializeField] private FastNoiseLite.TransformType3D _warpTransformType3D;
        [SerializeField] private float _domainWarpAmp;

        public FastNoiseLite.DomainWarpType DomainWarpType => _domainWarpType;
        public FastNoiseLite.TransformType3D WarpTransformType3D => _warpTransformType3D;
        public float DomainWarpAmp => _domainWarpAmp;
    }
}
