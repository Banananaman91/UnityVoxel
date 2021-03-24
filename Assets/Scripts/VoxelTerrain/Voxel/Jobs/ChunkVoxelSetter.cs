using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.Noise;

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
        [ReadOnly] public float StoneDepth;
        [ReadOnly] public float SnowHeight;
        [ReadOnly] public float CaveStartHeight;

        public Vector3 origin;
        public NativeArray<float> voxels;
        public int seed;

        public void Execute()
        {
            for (var i = 0; i < size; i++)
            {
                for (var k = 0; k < size; k++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        voxels[Chunk.PosToIndex(i, j, k)] = SetVoxelType(origin.x + i * resolution, origin.y + j * resolution,
                            origin.z + k * resolution);
                    }
                }
            }
        }

        //set individual voxel type using noise function
        public float SetVoxelType(float x, float y, float z)
        {
            var blockType = VoxelType.Default;

            //3D noise for heightmap
            var simplex1 = PerlinNoise.Generate2DNoiseValue( x, z, scale, seed, groundLevel);

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

            return (float) blockType;
        }
    }
}
