using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.Editor.Noise;

namespace VoxelTerrain.Editor.Voxel.Jobs
{
    public struct ChunkVoxelSetter : IJob
    {
        [ReadOnly] public int size;
        [ReadOnly] public int height;
        [ReadOnly] public int heightMultiplier;
        [ReadOnly] public float scale;
        [ReadOnly] public float resolution;

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
                        voxels[Chunk.PosToIndex(i, j, k)] = SetVoxelType(origin.x + (i * resolution), origin.y + (j * resolution),
                            origin.z + (k * resolution));
                    }
                }
            }
        }

        //set individual voxel type using noise function
        public float SetVoxelType(float x, float y, float z)
        {
            var blockType = VoxelType.Default;

            //3D noise for heightmap
            var simplex1 = PerlinNoise.Generate2DNoiseValue( x * 0.3f, z * 0.3f, scale, seed, 0) * heightMultiplier;
            var simplex2 = PerlinNoise.Generate2DNoiseValue(x * 0.8f, z * 0.8f, scale, seed, 0) * heightMultiplier;
            
            var heightMap = simplex1 + simplex2;

            //under the surface, dirt block
            if (y <= heightMap)
            {
                //blockType = VoxelType.Dirt;
                blockType = VoxelType.Dirt;

                //just on the surface, use a grass type
                if (y > heightMap - 1)
                {
                    blockType = VoxelType.Grass;
                }
            }

            return (float) blockType;
        }
    }
}
