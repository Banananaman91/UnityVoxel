﻿using TerrainData;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.DataConversion;

namespace VoxelTerrain.Engine.Jobs
{
    [BurstCompile]
    public struct ChunkVoxelSetter : IJob
    {
        [ReadOnly] public int size;
        [ReadOnly] public int height;
        [ReadOnly] public float heightScale;
        [ReadOnly] public float noise1Scale;
        [ReadOnly] public float noise2Scale;
        [ReadOnly] public float altitudeScale;
        [ReadOnly] public float moistureScale;
        [ReadOnly] public float resolution;
        [ReadOnly] public int octaves;
        [ReadOnly] public float lacunarity;
        [ReadOnly] public float amplitude;
        [ReadOnly] public float frequency;

        public Vector3 origin;
        public NativeArray<Voxel> voxels;
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
                        voxels[Converter.PosToIndex(i, j, k)] = BiomeGenerator.GenerateVoxelType(origin.x + i * resolution, origin.y + j * resolution, origin.z + k * resolution, noise1Scale, noise2Scale, heightScale, altitudeScale, moistureScale, seed, octaves, lacunarity, amplitude, frequency);
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
