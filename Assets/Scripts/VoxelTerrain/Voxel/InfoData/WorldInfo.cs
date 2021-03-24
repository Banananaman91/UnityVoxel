using UnityEngine;

namespace VoxelTerrain.Voxel.InfoData
{
    public class WorldInfo : MonoBehaviour
    {
        [SerializeField] private float _distance;
        [SerializeField] private Transform _origin;
        [SerializeField] private float _groundLevel;
        
        public float Distance => _distance;
        public Transform Origin => _origin;

        public float GroundLevel => _groundLevel;
    }
}
