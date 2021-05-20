using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using VoxelTerrain.Engine;
using VoxelTerrain.Engine.InfoData;

namespace TerrainData
{
    public class BiomeGenerator : MonoBehaviour
    {
        public static Voxel GenerateVoxelType(float x, float y, float z, NativeArray<NoiseInfo> noiseInfo, int seed)
        {
            float firstLayerValue = 0;
            float heightMap = 0;
            for (int i = 0; i < noiseInfo.Length; i++)
            {
                if (i == 0)
                {
                    firstLayerValue = Noise.GenerateSimple2DNoiseValue(x, z, noiseInfo[i].NoiseScale,
                        noiseInfo[i].Octaves, noiseInfo[i].Lacunarity, seed);
                    heightMap = firstLayerValue;
                    continue;
                }
                heightMap += Noise.GenerateSimple2DNoiseValue(x, z, noiseInfo[i].NoiseScale,
                    noiseInfo[i].Octaves, noiseInfo[i].Lacunarity, seed) * firstLayerValue;
            }
            // var altitude = Noise.GenerateSimple3DNoiseValue(x / altitudeScale, y / altitudeScale, z / altitudeScale, altitudeScale, octaves, lacunarity, seed);
            // var moisture = Noise.GenerateSimple3DNoiseValue(x / moistureScale, y / moistureScale, z / moistureScale, moistureScale, octaves, lacunarity,  seed + 1000);
            //
            // var simplex1 =
            //     Noise.GenerateSimple2DNoiseValue(x * 0.3f, z * 0.3f, noise1Scale, octaves, lacunarity, 
            //         seed) * noise1Scale;
            // var simplex2 =
            //     Noise.GenerateSimple2DNoiseValue(x * 0.3f, z * 0.3f, noise1Scale, octaves, lacunarity, 
            //         seed) * noise2Scale;
            //
            // var heightMap = simplex1;
            // heightMap += simplex2 * simplex1;
            var heightSample = heightMap - y;

            VoxelType voxelType = VoxelType.Grass;
            
            // // Ice
            // if (moisture > 6.5 && altitude <= -0.8 * heightScale)
            // {
            //     voxelType = VoxelType.Ice;
            // }
            // // Grass Water
            // else if (moisture > 3 && moisture <= 6.5 && altitude <= -0.8 * heightScale)
            // {
            //     voxelType = VoxelType.Water;
            // }
            // // Swamp Water
            // else if (moisture > 0 && moisture <= 3 && altitude <= -1.4 * heightScale)
            // {
            //     voxelType = VoxelType.Water;
            // }
            // // Swamp Water
            // else if (moisture > 0 && moisture <= 3 && altitude > -0.6 * heightScale && altitude <= -0.2 * heightScale)
            // {
            //     voxelType = VoxelType.Water;
            // }
            // // Jungle Water
            // else if (moisture > -3 && moisture <= 0 && altitude <= -0.8 * heightScale)
            // {
            //     voxelType = VoxelType.Water;
            // }
            // // Savannah Water
            // else if (moisture > -6.5 && moisture <= -3 && altitude <= -1.4 * heightScale)
            // {
            //     voxelType = VoxelType.Water;
            // }
            // // // Desert Canyons
            // // else if (moisture <= -6.5 && altitude <= -0.6 * heightScale && altitude > -1 * heightScale && y > -20)
            // // {
            // //     voxelType = VoxelType.Default;
            // // }
            // // Dirt
            // // else
            // // {
            // //     voxelType = VoxelType.Dirt;
            // // }
            //
            // if (y < heightMap * heightScale - 1)
            // {
            //     // Snow
            //     if (moisture > 6.5)
            //     {
            //         if (altitude > 1.6 * heightScale)
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //         else if (altitude > 1 * heightScale)
            //         {
            //             voxelType = VoxelType.Stone;
            //         }
            //         else if (altitude > 0.6 * heightScale)
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //         else if (altitude > 0 * heightScale)
            //         {
            //             voxelType = VoxelType.PineForest;
            //         }
            //         else if (altitude > -0.8 * heightScale)
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //         else
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //     }
            //     // Grass
            //     else if (moisture > 3)
            //     {
            //         if (altitude > 1.8 * heightScale)
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //         else if (altitude > 1 * heightScale)
            //         {
            //             voxelType = VoxelType.Stone;
            //         }
            //         else if (altitude > 0.6 * heightScale)
            //         {
            //             voxelType = VoxelType.Grass;
            //         }
            //         else if (altitude > 0 * heightScale)
            //         {
            //             voxelType = VoxelType.Forest;
            //         }
            //         else if (altitude > -0.75 * heightScale)
            //         {
            //             voxelType = VoxelType.Grass;
            //         }
            //         else if (altitude > -0.8 * heightScale)
            //         {
            //             voxelType = VoxelType.Beach;
            //         }
            //         else
            //         {
            //             voxelType = VoxelType.Grass;
            //         }
            //     }
            //     // Swamp
            //     else if (moisture > 0)
            //     {
            //         if (altitude > 1.8 * heightScale)
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //         else if (altitude > 1 * heightScale)
            //         {
            //             voxelType = VoxelType.Stone;
            //         }
            //         else if (altitude > 0.2 * heightScale)
            //         {
            //             voxelType = VoxelType.Grass;
            //         }
            //         else if (altitude > 0 * heightScale)
            //         {
            //             voxelType = VoxelType.SwampForest;
            //         }
            //         else if (altitude > -0.2 * heightScale)
            //         {
            //             voxelType = VoxelType.Mud;
            //         }
            //         else if (altitude > -0.6 * heightScale)
            //         {
            //             voxelType = VoxelType.SwampForest;
            //         }
            //         else if (altitude > -0.7 * heightScale)
            //         {
            //             voxelType = VoxelType.Mud;
            //         }
            //         else if (altitude > -1.1 * heightScale)
            //         {
            //             voxelType = VoxelType.SwampForest;
            //         }
            //         else if (altitude > -1.4 * heightScale)
            //         {
            //             voxelType = VoxelType.Mud;
            //         }
            //         else
            //         {
            //             voxelType = VoxelType.SwampForest;
            //         }
            //     }
            //     // Jungle
            //     else if (moisture > -3)
            //     {
            //         if (altitude > 1.8 * heightScale)
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //         else if (altitude > 1 * heightScale)
            //         {
            //             voxelType = VoxelType.Stone;
            //         }
            //         else if (altitude > -0.6 * heightScale)
            //         {
            //             voxelType = VoxelType.JungleForest;
            //         }
            //         else if (altitude > -0.8 * heightScale)
            //         {
            //             voxelType = VoxelType.Beach;
            //         }
            //         else
            //         {
            //             voxelType = VoxelType.JungleForest;
            //         }
            //     }
            //     // Savannah
            //     else if (moisture > -6.5)
            //     {
            //         if (altitude > 1.8 * heightScale)
            //         {
            //             voxelType = VoxelType.Snow;
            //         }
            //         else if (altitude > 1 * heightScale)
            //         {
            //             voxelType = VoxelType.Stone;
            //         }
            //         else if (altitude > 0.2 * heightScale)
            //         {
            //             voxelType = VoxelType.SavannahGrass;
            //         }
            //         else if (altitude > -1 * heightScale)
            //         {
            //             voxelType = VoxelType.Plains;
            //         }
            //         else if (altitude > -1.4 * heightScale)
            //         {
            //             voxelType = VoxelType.SavannahForest;
            //         }
            //         else
            //         {
            //             voxelType = VoxelType.SavannahForest;
            //         }
            //     }
            //     // Desert
            //     else
            //     {
            //         if (altitude > 1 * heightScale)
            //         {
            //             voxelType = VoxelType.Sandstone;
            //         }
            //         else if (altitude > -0.6 * heightScale)
            //         {
            //             voxelType = VoxelType.Sand;
            //         }
            //         else if (altitude > -1 * heightScale)
            //         {
            //             voxelType = VoxelType.Sandstone;
            //         }
            //         else
            //         {
            //             voxelType = VoxelType.Sand;
            //         }
            //     }
            // }


            return new Voxel((byte) voxelType, heightSample);
        }
    }
}
