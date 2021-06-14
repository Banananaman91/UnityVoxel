using TerrainData;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.DataConversion;
using VoxelTerrain.Engine.InfoData;

namespace VoxelTerrain.Engine.Jobs
{
    [BurstCompile]
    public struct ChunkVoxelSetter : IJob
    {
        [ReadOnly] public int size;
        [ReadOnly] public int height;
        [ReadOnly] public int seed;
        [ReadOnly] public NativeArray<NoiseInfo> noiseData;
        [ReadOnly] public float resolution;
        public Vector3 origin;
        public NativeArray<Voxel> voxels;

        public void Execute()
        {
            for (var i = 0; i <= size; i++)
            {
                for (var k = 0; k <= size; k++)
                {
                    for (var j = 0; j < height; j++)
                    {
                        //set voxel based on noise world position
                        voxels[Converter.PosToIndex(i, j, k)] = BiomeGenerator.GenerateVoxelType(origin.x + i * resolution, origin.y + j * resolution, origin.z + k * resolution, noiseData, seed);
                    }
                }
            }
        }
    }
}
