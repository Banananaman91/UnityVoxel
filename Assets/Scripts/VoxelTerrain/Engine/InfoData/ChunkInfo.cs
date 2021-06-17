using UnityEngine;

namespace VoxelTerrain.Engine.InfoData
{
    public class ChunkInfo : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private GameObject _chunkPrefab;
        [SerializeField] private ComputeShader _marchingShader;
        [SerializeField] private ComputeShader _noiseShader;
        [SerializeField, Range(0.01f, 1.0f), Tooltip("Metre cubed")] private float _voxelSize;
        [SerializeField] private bool _interpolateMesh;
#pragma warning restore 0649

        public GameObject ChunkPrefab => _chunkPrefab;
        public float VoxelSize => _voxelSize;
        public bool InterpolateMesh => _interpolateMesh;

        public ComputeShader MarchingShader => _marchingShader;
        public ComputeShader NoiseShader => _noiseShader;
    }
}
