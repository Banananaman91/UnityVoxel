using UnityEngine;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    [System.Serializable]
    public struct ChunkInfo
    {
        [SerializeField] private MonoChunk _chunkPrefab;
        [SerializeField, Range(0.01f, 1.0f), Tooltip("Metre cubed")] private float _voxelSize;

        public MonoChunk ChunkPrefab => _chunkPrefab;
        public float VoxelSize => _voxelSize;
    }
}
