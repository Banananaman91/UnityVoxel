using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TerrainData
{
    public class BiomeGenerator : MonoBehaviour
    {
        public static byte GenerateVoxelType(float x, float y, float z, float scale, int seed, float groundLevel)
        {
            float altitude = Noise.Generate2DNoiseValue(x * 0.5f, z * 0.5f, scale, 4, 2f, seed, groundLevel);
            float moisture = Noise.Generate2DNoiseValue(x * 0.025f, z * 0.025f, scale, 6, 3f, seed + 1000, 0);

            VoxelTerrain.Voxel.VoxelType voxelType = new VoxelTerrain.Voxel.VoxelType();

            float heightScale = 5;

            // Set the value at the current coordinate and subtract ground level
            float groundAltitude = altitude - (groundLevel * scale);

            // Anything below ground level is moved up to 0 for flat land
            if (groundAltitude < 0)
            {
                groundAltitude = 0;
            }

            if (y <= groundAltitude)
            {
                // Ice
                if (moisture > 6.5 && altitude <= -0.8 * heightScale)
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Ice;
                }
                // Grass Water
                else if (moisture > 3 && moisture <= 6.5 && altitude <= -0.8 * heightScale)
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Water;
                }
                // Swamp Water
                else if (moisture > 0 && moisture <= 3 && altitude <= -1.4 * heightScale)
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Water;
                }
                // Swamp Water
                else if (moisture > 0 && moisture <= 3 && altitude > -0.6 * heightScale && altitude <= -0.2 * heightScale)
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Water;
                }
                // Jungle Water
                else if (moisture > -3 && moisture <= 0 && altitude <= -0.8 * heightScale)
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Water;
                }
                // Savannah Water
                else if (moisture > -6.5 && moisture <= -3 && altitude <= -1.4 * heightScale)
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Water;
                }
                // Desert Canyons
                else if (moisture <= -6.5 && altitude <= -0.6 * heightScale && altitude > -1 * heightScale && y > -20)
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                }
                // Dirt
                else
                {
                    voxelType = VoxelTerrain.Voxel.VoxelType.Dirt;
                }

                if (y > groundAltitude - 1)
                {
                    // Snow
                    if (moisture > 6.5)
                    {
                        if (altitude > 1.6 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Stone;
                        }
                        else if (altitude > 0.6 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Snow;
                        }
                        else if (altitude > 0 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.PineForest;
                        }
                        else if (altitude > -0.8 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Snow;
                        }
                        else
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                        }
                    }
                    // Grass
                    else if (moisture > 3)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Stone;
                        }
                        else if (altitude > 0.6 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Grass;
                        }
                        else if (altitude > 0 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Forest;
                        }
                        else if (altitude > -0.75 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Grass;
                        }
                        else if (altitude > -0.8 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Beach;
                        }
                        else
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                        }
                    }
                    // Swamp
                    else if (moisture > 0)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Stone;
                        }
                        else if (altitude > 0.2 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Grass;
                        }
                        else if (altitude > 0 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.SwampForest;
                        }
                        else if (altitude > -0.2 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Mud;
                        }
                        else if (altitude > -0.6 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                        }
                        else if (altitude > -0.7 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Mud;
                        }
                        else if (altitude > -1.1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.SwampForest;
                        }
                        else if (altitude > -1.4 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Mud;
                        }
                        else
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                        }
                    }
                    // Jungle
                    else if (moisture > -3)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Stone;
                        }
                        else if (altitude > -0.6 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.JungleForest;
                        }
                        else if (altitude > -0.8 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Beach;
                        }
                        else
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                        }
                    }
                    // Savannah
                    else if (moisture > -6.5)
                    {
                        if (altitude > 1.8 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Snow;
                        }
                        else if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Stone;
                        }
                        else if (altitude > 0.2 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.SavannahGrass;
                        }
                        else if (altitude > -1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Plains;
                        }
                        else if (altitude > -1.4 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.SavannahForest;
                        }
                        else
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                        }
                    }
                    // Desert
                    else
                    {
                        if (altitude > 1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Sandstone;
                        }
                        else if (altitude > -0.6 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Sand;
                        }
                        else if (altitude > -1 * heightScale)
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Default;
                        }
                        else
                        {
                            voxelType = VoxelTerrain.Voxel.VoxelType.Sand;
                        }
                    }
                }
            }

            return (byte) voxelType;
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
