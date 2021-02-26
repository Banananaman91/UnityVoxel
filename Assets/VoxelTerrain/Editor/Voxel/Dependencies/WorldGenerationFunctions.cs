using System;
using UnityEngine;
using VoxelTerrain.Dependencies;

namespace VoxelTerrain.Editor.Voxel.Dependencies
{
    public class WorldGenerationFunctions : MonoBehaviour
    {
        [SerializeField] private World _world;
        [SerializeField] private VoxelEngine _engine;
        [SerializeField] private ChunkGenerator _chunkGenerator;

        public ChunkGenerator ChunkGenerator => _chunkGenerator;

        public void GenerateWorld(Vector3 origin, float distance, float size)
        {
            _chunkGenerator.engine = _engine;
            for (float x = origin.x - distance; x <= origin.x + distance; x += Chunk.ChunkSize)
            {
                for (float z = origin.z - distance; z <= origin.z + distance; z += Chunk.ChunkSize)
                {
                    GenerateChunkData(new Vector3(x, 0, z));
                }
            }
        }
        
        private void GenerateChunkData(Vector3 pos) => _chunkGenerator.CreateChunkJob(pos);
    }
}
