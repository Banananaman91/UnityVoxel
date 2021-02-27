using UnityEngine;

namespace VoxelTerrain.Editor.Voxel.InfoData
{
    public class ChunkInfo : MonoBehaviour
    {
        [SerializeField] private MonoChunk _chunkPrefab;
        [SerializeField, Range(0.01f, 1.0f), Tooltip("Metre cubed")] private float _voxelSize;

        public MonoChunk ChunkPrefab => _chunkPrefab;
        public float VoxelSize => _voxelSize;
    }
}
