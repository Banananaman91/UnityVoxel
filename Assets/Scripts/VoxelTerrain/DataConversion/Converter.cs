using UnityEngine;
using VoxelTerrain.Engine;

namespace VoxelTerrain.DataConversion
{
    public static class Converter
    {
        private static int ChunkSize => Chunk.ChunkSize;
        private static int ChunkHeight => Chunk.ChunkHeight;
        
        //convert world position to index position of the voxel array
        public static int PosToIndex(int x, int y, int z) => z * (ChunkSize) * (ChunkHeight) + y * (ChunkSize) + x;

        public static Vector3 IndexToPos(int idx)
        {
            var z = idx / (Chunk.ChunkSize * Chunk.ChunkHeight);
            idx -= (z * Chunk.ChunkSize * Chunk.ChunkHeight);
            var y = idx / Chunk.ChunkSize;
            var x = idx % Chunk.ChunkSize;
            return new Vector3(x, y, z);
        }
    }
}
