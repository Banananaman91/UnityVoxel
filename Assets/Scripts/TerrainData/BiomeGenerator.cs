﻿using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using VoxelTerrain.Engine;

namespace TerrainData
{
    public class BiomeGenerator : MonoBehaviour
    {
        public static Voxel GenerateVoxelType(float x, float y, float z, float scale, int seed, float groundLevel, int octaves, float lacunarity, float amplitude, float frequency)
        {
            float heightScale = 5;
            float altitude = Noise.Generate3DNoiseValue(x * 0.5f, y * 0.5f, z * 0.5f, scale, octaves, lacunarity, amplitude, frequency, seed) * scale;
            float moisture = Noise.Generate3DNoiseValue(x * 0.025f,y * 0.025f, z * 0.025f, scale, octaves, lacunarity, amplitude, frequency, seed + 1000) * scale;
            
            var simplex1 = Noise.Generate2DNoiseValue(x * 0.8f,  z * 0.8f, scale, octaves, lacunarity, amplitude, frequency, seed) * scale;
            var simplex2 = Noise.Generate2DNoiseValue(x * 0.3f,  z * 0.3f, scale, octaves, lacunarity, amplitude, frequency, seed) * scale;
            var heightMap = (simplex1 + simplex2);
            var heightSample = heightMap - y;
            var volumetricSample = Noise.Generate3DNoiseValue(x + math.PI * 0.5f, y + math.PI * 0.5f, z + math.PI * 0.5f, scale, octaves, lacunarity, amplitude, frequency, seed);

            var result = math.min(heightSample, -volumetricSample) + math.clamp(heightMap, 0, 1);
            
            moisture *= scale;
            altitude *= scale;

            VoxelType voxelType = VoxelType.Default;
            
            // // Set the value at the current coordinate and subtract ground level
            // float groundAltitude = altitude * scale - (groundLevel * scale);
            //
            // // Anything below ground level is moved up to 0 for flat land
            // if (groundAltitude < 0)
            // {
            //     groundAltitude = 0;
            // }

            if (y <= heightMap * scale)
            {
                // Ice
                if (moisture > 6.5 && altitude <= -0.8 * heightScale)
                {
                    voxelType = VoxelType.Ice;
                }
                // Grass Water
                else if (moisture > 3 && moisture <= 6.5 && altitude <= -0.8 * heightScale)
                {
                    voxelType = VoxelType.Water;
                }
                // Swamp Water
                else if (moisture > 0 && moisture <= 3 && altitude <= -1.4 * heightScale)
                {
                    voxelType = VoxelType.Water;
                }
                // Swamp Water
                else if (moisture > 0 && moisture <= 3 && altitude > -0.6 * heightScale && altitude <= -0.2 * heightScale)
                {
                    voxelType = VoxelType.Water;
                }
                // Jungle Water
                else if (moisture > -3 && moisture <= 0 && altitude <= -0.8 * heightScale)
                {
                    voxelType = VoxelType.Water;
                }
                // Savannah Water
                else if (moisture > -6.5 && moisture <= -3 && altitude <= -1.4 * heightScale)
                {
                    voxelType = VoxelType.Water;
                }
                // Desert Canyons
                else if (moisture <= -6.5 && altitude <= -0.6 * heightScale && altitude > -1 * heightScale && y > -20)
                {
                    voxelType = VoxelType.Default;
                }
                // Dirt
                else
                {
                    voxelType = VoxelType.Dirt;
                }

                if (y < heightMap * scale - 1)
                {
                    // Snow
                    if (moisture > 6.5)
                    {
                        if (altitude > 1.6 * heightScale)
                        {
                            voxelType = VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelType.Stone;
                        }
                        else if (altitude > 0.6 * heightScale)
                        {
                            voxelType = VoxelType.Snow;
                        }
                        else if (altitude > 0 * heightScale)
                        {
                            voxelType = VoxelType.PineForest;
                        }
                        else if (altitude > -0.8 * heightScale)
                        {
                            voxelType = VoxelType.Snow;
                        }
                        else
                        {
                            voxelType = VoxelType.Default;
                        }
                    }
                    // Grass
                    else if (moisture > 3)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelType.Stone;
                        }
                        else if (altitude > 0.6 * heightScale)
                        {
                            voxelType = VoxelType.Grass;
                        }
                        else if (altitude > 0 * heightScale)
                        {
                            voxelType = VoxelType.Forest;
                        }
                        else if (altitude > -0.75 * heightScale)
                        {
                            voxelType = VoxelType.Grass;
                        }
                        else if (altitude > -0.8 * heightScale)
                        {
                            voxelType = VoxelType.Beach;
                        }
                        else
                        {
                            voxelType = VoxelType.Default;
                        }
                    }
                    // Swamp
                    else if (moisture > 0)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelType.Stone;
                        }
                        else if (altitude > 0.2 * heightScale)
                        {
                            voxelType = VoxelType.Grass;
                        }
                        else if (altitude > 0 * heightScale)
                        {
                            voxelType = VoxelType.SwampForest;
                        }
                        else if (altitude > -0.2 * heightScale)
                        {
                            voxelType = VoxelType.Mud;
                        }
                        else if (altitude > -0.6 * heightScale)
                        {
                            voxelType = VoxelType.Default;
                        }
                        else if (altitude > -0.7 * heightScale)
                        {
                            voxelType = VoxelType.Mud;
                        }
                        else if (altitude > -1.1 * heightScale)
                        {
                            voxelType = VoxelType.SwampForest;
                        }
                        else if (altitude > -1.4 * heightScale)
                        {
                            voxelType = VoxelType.Mud;
                        }
                        else
                        {
                            voxelType = VoxelType.Default;
                        }
                    }
                    // Jungle
                    else if (moisture > -3)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelType.Stone;
                        }
                        else if (altitude > -0.6 * heightScale)
                        {
                            voxelType = VoxelType.JungleForest;
                        }
                        else if (altitude > -0.8 * heightScale)
                        {
                            voxelType = VoxelType.Beach;
                        }
                        else
                        {
                            voxelType = VoxelType.Default;
                        }
                    }
                    // Savannah
                    else if (moisture > -6.5)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelType.Stone;
                        }
                        else if (altitude > 0.2 * heightScale)
                        {
                            voxelType = VoxelType.SavannahGrass;
                        }
                        else if (altitude > -1 * heightScale)
                        {
                            voxelType = VoxelType.Plains;
                        }
                        else if (altitude > -1.4 * heightScale)
                        {
                            voxelType = VoxelType.SavannahForest;
                        }
                        else
                        {
                            voxelType = VoxelType.Default;
                        }
                    }
                    // Desert
                    else
                    {
                        if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelType.Sandstone;
                        }
                        else if (altitude > -0.6 * heightScale)
                        {
                            voxelType = VoxelType.Sand;
                        }
                        else if (altitude > -1 * heightScale)
                        {
                            voxelType = VoxelType.Default;
                        }
                        else
                        {
                            voxelType = VoxelType.Sand;
                        }
                    }
                }
            }

            return new Voxel((byte) voxelType, result);
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
