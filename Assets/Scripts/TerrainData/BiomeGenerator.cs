using Unity.Collections;
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
            for (var i = 0; i < noiseInfo.Length; i++)
            {
                float v;

                switch (noiseInfo[i].NoiseType)
                {
                    case 0:
                        v = noiseInfo[i].ThreeDimensional == 1 ? Noise.GenerateSimple3DNoiseValue(x, y, z, noiseInfo[i].NoiseScale,
                                noiseInfo[i].Octaves, noiseInfo[i].Lacunarity, noiseInfo[i].Dimension, seed) *
                            noiseInfo[i].HeightScale : Noise.GenerateSimple2DNoiseValue(x, z, noiseInfo[i].NoiseScale,
                                                           noiseInfo[i].Octaves, noiseInfo[i].Lacunarity, noiseInfo[i].Dimension, seed) *
                                                       noiseInfo[i].HeightScale;
                        break;
                    case 1:
                        v = noiseInfo[i].ThreeDimensional == 1 ? Noise.GenerateRigid3DNoiseValue(x, y, z, noiseInfo[i].NoiseScale,
                                                                noiseInfo[i].Octaves, noiseInfo[i].Lacunarity, noiseInfo[i].Dimension, seed) *
                                                            noiseInfo[i].HeightScale : Noise.GenerateRigid2DNoiseValue(x, z, noiseInfo[i].NoiseScale,
                                noiseInfo[i].Octaves, noiseInfo[i].Lacunarity, noiseInfo[i].Dimension, seed) *
                            noiseInfo[i].HeightScale;
                        break;
                    default:
                        v = 1;
                        break;
                }
                
                if (i == 0)
                {
                    firstLayerValue = v;
                    heightMap = firstLayerValue;
                    continue;
                }
                heightMap += v * firstLayerValue;
            }
            var heightSample = heightMap - y;
            
            var altitude = heightSample;
            var moisture = Noise.GenerateSimple3DNoiseValue(x * 0.025f, y * 0.025f, z * 0.025f, 5, 4, 2, 0.5f, seed + 1000);

            VoxelType voxelType = VoxelType.Grass;
            // Ice
            if (moisture > 6.5 && altitude <= -0.8)
            {
                voxelType = VoxelType.Ice;
            }
            // Grass Water
            else if (moisture > 3 && moisture <= 6.5 && altitude <= -0.8)
            {
                voxelType = VoxelType.Water;
            }
            // Swamp Water
            else if (moisture > 0 && moisture <= 3 && altitude <= -1.4)
            {
                voxelType = VoxelType.Water;
            }
            // Swamp Water
            else if (moisture > 0 && moisture <= 3 && altitude > -0.6 && altitude <= -0.2)
            {
                voxelType = VoxelType.Water;
            }
            // Jungle Water
            else if (moisture > -3 && moisture <= 0 && altitude <= -0.8)
            {
                voxelType = VoxelType.Water;
            }
            // Savannah Water
            else if (moisture > -6.5 && moisture <= -3 && altitude <= -1.4)
            {
                voxelType = VoxelType.Water;
            }

            if (y < heightMap - 1)
            {
                // Snow
                if (moisture > 6.5)
                {
                    if (altitude > 1.6)
                    {
                        voxelType = VoxelType.Snow;
                    }
                    else if (altitude > 1)
                    {
                        voxelType = VoxelType.Stone;
                    }
                    else if (altitude > 0.6)
                    {
                        voxelType = VoxelType.Snow;
                    }
                    else if (altitude > 0)
                    {
                        voxelType = VoxelType.PineForest;
                    }
                    else if (altitude > -0.8)
                    {
                        voxelType = VoxelType.Snow;
                    }
                    else
                    {
                        voxelType = VoxelType.Snow;
                    }
                }
                // Grass
                else if (moisture > 3)
                {
                    if (altitude > 1.8)
                    {
                        voxelType = VoxelType.Snow;
                    }
                    else if (altitude > 1)
                    {
                        voxelType = VoxelType.Stone;
                    }
                    else if (altitude > 0.6)
                    {
                        voxelType = VoxelType.Grass;
                    }
                    else if (altitude > 0)
                    {
                        voxelType = VoxelType.Forest;
                    }
                    else if (altitude > -0.75)
                    {
                        voxelType = VoxelType.Grass;
                    }
                    else if (altitude > -0.8)
                    {
                        voxelType = VoxelType.Beach;
                    }
                    else
                    {
                        voxelType = VoxelType.Grass;
                    }
                }
                // Swamp
                else if (moisture > 0)
                {
                    if (altitude > 1.8)
                    {
                        voxelType = VoxelType.Snow;
                    }
                    else if (altitude > 1)
                    {
                        voxelType = VoxelType.Stone;
                    }
                    else if (altitude > 0.2)
                    {
                        voxelType = VoxelType.Grass;
                    }
                    else if (altitude > 0)
                    {
                        voxelType = VoxelType.SwampForest;
                    }
                    else if (altitude > -0.2)
                    {
                        voxelType = VoxelType.Mud;
                    }
                    else if (altitude > -0.6)
                    {
                        voxelType = VoxelType.SwampForest;
                    }
                    else if (altitude > -0.7)
                    {
                        voxelType = VoxelType.Mud;
                    }
                    else if (altitude > -1.1)
                    {
                        voxelType = VoxelType.SwampForest;
                    }
                    else if (altitude > -1.4)
                    {
                        voxelType = VoxelType.Mud;
                    }
                    else
                    {
                        voxelType = VoxelType.SwampForest;
                    }
                }
                // Jungle
                else if (moisture > -3)
                {
                    if (altitude > 1.8)
                    {
                        voxelType = VoxelType.Snow;
                    }
                    else if (altitude > 1)
                    {
                        voxelType = VoxelType.Stone;
                    }
                    else if (altitude > -0.6)
                    {
                        voxelType = VoxelType.JungleForest;
                    }
                    else if (altitude > -0.8)
                    {
                        voxelType = VoxelType.Beach;
                    }
                    else
                    {
                        voxelType = VoxelType.JungleForest;
                    }
                }
                // Savannah
                else if (moisture > -6.5)
                {
                    if (altitude > 1.8)
                    {
                        voxelType = VoxelType.Snow;
                    }
                    else if (altitude > 1)
                    {
                        voxelType = VoxelType.Stone;
                    }
                    else if (altitude > 0.2)
                    {
                        voxelType = VoxelType.SavannahGrass;
                    }
                    else if (altitude > -1)
                    {
                        voxelType = VoxelType.Plains;
                    }
                    else if (altitude > -1.4)
                    {
                        voxelType = VoxelType.SavannahForest;
                    }
                    else
                    {
                        voxelType = VoxelType.SavannahForest;
                    }
                }
                // Desert
                else
                {
                    if (altitude > 1)
                    {
                        voxelType = VoxelType.Sandstone;
                    }
                    else if (altitude > -0.6)
                    {
                        voxelType = VoxelType.Sand;
                    }
                    else if (altitude > -1)
                    {
                        voxelType = VoxelType.Sandstone;
                    }
                    else
                    {
                        voxelType = VoxelType.Sand;
                    }
                }
            }


            return new Voxel((byte) voxelType, heightSample);
        }
    }
}
