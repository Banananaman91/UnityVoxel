using UnityEngine;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    public class NoiseValues : MonoBehaviour
    {
        [SerializeField] private float _simplexOneScale;
        [SerializeField] private float _simplexTwoScale;
        [SerializeField] private float _caveNoiseOneScale;
        [SerializeField] private float _caveNoiseTwoScale;
        [SerializeField] private float _caveMask;
        [SerializeField] private float _simplexStoneOneScale;
        [SerializeField] private float _simplexStoneTwoScale;

        public float SimplexOneScale => _simplexOneScale;
        public float SimplexTwoScale => _simplexTwoScale;
        public float CaveNoiseOneScale => _caveNoiseOneScale;
        public float CaveNoiseTwoScale => _caveNoiseTwoScale;
        public float CaveMask => _caveMask;
        public float SimplexStoneOneScale => _simplexStoneOneScale;
        public float SimplexStoneTwoScale => _simplexStoneTwoScale;
    }
}
