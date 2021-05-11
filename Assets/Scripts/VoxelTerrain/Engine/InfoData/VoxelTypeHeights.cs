using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    public class VoxelTypeHeights : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float _stoneDepth;
        [SerializeField] private float _snowHeight;
        [SerializeField] private float _caveStartHeight;
#pragma warning restore 0649

        public float StoneDepth => _stoneDepth;
        public float SnowHeight => _snowHeight;
        public float CaveStartHeight => _caveStartHeight;
    }
}
