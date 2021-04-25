using UnityEngine;
using VoxelTerrain.SaveLoad;

namespace VoxelTerrain.Voxel.Dependencies
{
    [RequireComponent(typeof(ChunkGenerator), typeof(ChunkLoader))]
    public class WorldGenerationFunctions : MonoBehaviour
    {
        [SerializeField] private VoxelEngine _engine;
        [SerializeField] private ChunkGenerator _chunkGenerator;
        [SerializeField] private ChunkLoader _chunkLoader;

        public ChunkLoader ChunkLoader => _chunkLoader;

        public void GenerateWorld(Vector3 origin, float yPos, float distance, float size)
        {
            _chunkGenerator.Engine = _engine;
            for (float x = origin.x - distance; x <= origin.x + distance; x += size)
            {
                for (float z = origin.z - distance; z <= origin.z + distance; z += size)
                {
                    GenerateChunkData(new Vector3(x, yPos, z));
                }
            }
        }

        //Checking and creating chunk data
        public Chunk GenerateChunkData(Vector3 pos)
        {
            Chunk c;
            
            //Check if data exists already
            c = _chunkLoader.LoadChunkAt(pos);

            if (c != null) return c;
            
            //if no data is found, create some
            c = _chunkGenerator.CreateChunkJob(pos);

            return c;
        }
    }
}
