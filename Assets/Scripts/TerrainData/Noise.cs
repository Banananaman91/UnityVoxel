using SimplexNoise;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainData
{
    public static class Noise
    {
        // Mostly fixed variables for multiple octaves
        //private static readonly int octaves = 4;
        private static readonly float dimension = 0.5f; // Amplitude decrease each octave
        //private static readonly float lacunarity = 2; // Frequency increase each octave

        /// <summary>
        /// Outputs noise data using PRNG and multiple octaves
        /// </summary>
        /// <param name="width">Size of the map on the X axis</param>
        /// <param name="height">Size of the map on the Y axis</param>
        /// <param name="scale">Zoom level of the noise when displayed</param>
        /// <param name="seed">Seed of the generation</param>
        /// <param name="groundLevel">Ground level to limit lowest value</param>
        /// <param name="viewPos">Displayed position of the map</param>
        /// <returns>Noise values in 2D array</returns>
        public static float[,] GenerateNoiseMap(int width, int height, float scale, int octaves, float lacunarity, int seed, float groundLevel, Vector2 viewPos)
        {
            // 2D array to store noise values
            float[,] noiseMap = new float[width, height];

            // Nested loop through coordinates
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Generate a singular noise sample for this coordinate
                    noiseMap[x, y] = GenerateSample(new float3(x, y, 0), scale, seed, groundLevel, viewPos, octaves, lacunarity, false);
                }
            }

            return noiseMap;
        }

        /// <summary>
        /// Outputs a singular 2D noise value using PRNG and multiple octaves
        /// </summary>
        /// <param name="x">X coordinate of value</param>
        /// <param name="y">Y coordinate of value</param>
        /// <param name="scale">Zoom level of the noise when displayed</param>
        /// <param name="seed">Seed of the generation</param>
        /// <param name="groundLevel">Ground level to limit lowest value</param>
        /// <returns>Singular 2D noise value</returns>
        public static float Generate2DNoiseValue(float x, float y, float scale, int octaves, float lacunarity, int seed, float groundLevel)
        {
            return GenerateSample(new float3(x, y, 0), scale, seed, groundLevel, Vector2.zero, octaves, lacunarity, false);
        }

        /// <summary>
        /// Generates a singular 3D noise value using PRNG and multiple octaves
        /// </summary>
        /// <param name="x">X coordinate of sample</param>
        /// <param name="y">Y coordinate of sample</param>
        /// <param name="z">Z coordinate of sample</param>
        /// <param name="scale">Zoom level of the noise when displayed</param>
        /// <param name="seed">Seed of the generation</param>
        /// <returns>Singular 3D noise value</returns>
        public static float Generate3DNoiseValue(float x, float y, float z, float scale, int octaves, float lacunarity, int seed, float groundLevel)
        {
            return GenerateSample(new float3(x, y, z), scale, seed, groundLevel, Vector2.zero, octaves, lacunarity, true);
        }

        /// <summary>
        /// Generates a singular noise sample using PRNG and multiple octaves
        /// </summary>
        /// <param name="coords">Coordinates of sample</param>
        /// <param name="scale">Zoom level of the noise when displayed</param>
        /// <param name="seed">Seed of the generation</param>
        /// <param name="groundLevel">Ground level to limit lowest value</param>
        /// <param name="viewPos">Displayed position of the map</param>
        /// <param name="threeDimensions">Is the noise 3D</param>
        /// <returns>Singular 2D noise sample</returns>
        private static float GenerateSample(float3 coords, float scale, int seed,  float groundLevel, Vector2 viewPos, int octaves, float lacunarity, bool threeDimensions)
        {
            float noiseReturn = 0;

            // Local variables per coordinate
            float amplitude = 0.8f; // Vertical scale of noise
            float frequency = 0.3f; // Horizontal scale of noise

            for (int i = 0; i < octaves; i++)
            {
                // Find the sample coordinates to use in the noise function
                float xSample = coords.x * frequency;
                float ySample = coords.y * frequency;

                // Specific dimension requirements
                if (threeDimensions)
                {
                    // Find z sample
                    float zSample = coords.z * frequency;

                    // Generate 3D noise value
                    noiseReturn += FastNoiseLite.SingleOpenSimplex2S(seed + i, xSample, ySample, zSample) * amplitude;
                }
                else
                {
                    // Add view position to x and y samples
                    xSample += viewPos.x;
                    ySample += viewPos.y;

                    // Generate 2D noise value
                    noiseReturn += FastNoiseLite.SingleOpenSimplex2S(seed + i, xSample, ySample) * amplitude;

                }

                // Multiply the amplitude and frequency each octave
                amplitude *= dimension;
                frequency *= lacunarity;
            }

            /*
            if (!threeDimensions)
            {
                // Set the value at the current coordinate and subtract ground level
                noiseReturn -= groundLevel;

                // Anything below ground level is moved up to 0 for flat land
                if (noiseReturn < 0)
                {
                    noiseReturn = 0;
                }
            }
            */

            return noiseReturn;
        }

        /*
        /// <summary>
        /// Generates 3D Perlin noise value from given samples
        /// </summary>
        /// <param name="x">X noise sample</param>
        /// <param name="y">Y noise sample</param>
        /// <param name="z">Z noise sample</param>
        /// <returns>3D Perlin noise value</returns>
        public static float PerlinNoise3D(float x, float y, float z)
        {
            // Generate 2D noise value on every axis
            float xy = Mathf.PerlinNoise(x, y);
            float yz = Mathf.PerlinNoise(y, z);
            float xz = Mathf.PerlinNoise(x, z);
            float yx = Mathf.PerlinNoise(y, x);
            float zy = Mathf.PerlinNoise(z, y);
            float zx = Mathf.PerlinNoise(z, x);

            // Combine them and divide down to one noise value
            return (xy + yz + xz + yx + zy + zx) / 6;
        }
        */
    }
}
