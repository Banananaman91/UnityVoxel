using TerrainData;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace VoxelTerrain.Voxel.Jobs
{
    [BurstCompile]
    public struct ChunkVoxelSetter : IJob
    {
        [ReadOnly] public int size;
        [ReadOnly] public int height;
        [ReadOnly] public float groundLevel;
        [ReadOnly] public float scale;
        [ReadOnly] public float resolution;

        public Vector3 origin;
        public NativeArray<byte> voxels;
        public int seed;

        public void Execute()
        {
            for (var i = 0; i < size; i++)
            {
                for (var k = 0; k < size; k++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        //set voxel based on noise world position
                        voxels[Chunk.PosToIndex(i, j, k)] = BiomeGenerator.GenerateVoxelType(origin.x + i * resolution, origin.y + j * resolution, origin.z + k * resolution, scale, seed, groundLevel);
                    }
                }
            }
        }

        /*
        //set individual voxel type using noise function
        //eventually this will be replaced by Josephs noise types
        public byte SetVoxelType(float x, float y, float z)
        {
            var blockType = VoxelType.Default;

            //3D noise for heightmap
            var simplex1 = Noise.Generate2DNoiseValue( x, z, scale, numGen, groundLevel);

            //under the surface, dirt block
            if (y <= simplex1)
            {
                //blockType = VoxelType.Dirt;
                blockType = VoxelType.Dirt;

                //just on the surface, use a grass type
                if (y > simplex1 - 1)
                {
                    blockType = VoxelType.Grass;
                }
            }

            return (byte) blockType;
        }
        */
    }
}
