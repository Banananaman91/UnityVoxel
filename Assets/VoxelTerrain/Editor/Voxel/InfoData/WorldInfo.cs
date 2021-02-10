using UnityEngine;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    [System.Serializable]
    public struct WorldInfo
    {
        [SerializeField] private Material _material;
        [SerializeField] private float _distance;
        [SerializeField, Tooltip("Must be divisible by 32")] private float _height;
        [SerializeField] private Transform _origin;

        public Material Material => _material;
        public float Distance => _distance;
        public float Height => _height;
        public Transform Origin => _origin;
    }
}
