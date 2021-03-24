using UnityEngine;

namespace VoxelTerrain.Voxel.InfoData
{
    public class VoxelTypeHeights : MonoBehaviour
    {
        [SerializeField] private float _stoneDepth;
        [SerializeField] private float _snowHeight;
        [SerializeField] private float _caveStartHeight;

        public float StoneDepth => _stoneDepth;
        public float SnowHeight => _snowHeight;
        public float CaveStartHeight => _caveStartHeight;
    }
}
