using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    public class WorldInfo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private float _distance;
        [SerializeField] private Transform _origin;
        [SerializeField] private float _groundLevel;
        [SerializeField] private int _seed;
#pragma warning restore 0649
        
        public float Distance => _distance;
        public Transform Origin
        {
            get => _origin;
            set => _origin = value;
        }
        public float GroundLevel => _groundLevel;
        public int Seed
        {
            get => _seed;
            set => _seed = value;
        }
    }
}
