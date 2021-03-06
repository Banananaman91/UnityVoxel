﻿using System;
using SimplexNoise;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainData
{
    public static class Noise
    {
        // Mostly fixed variables for multiple octaves
        //private static readonly int octaves = 4;
        //private static readonly float dimension = 0.5f; // Amplitude decrease each octave
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
        public static float[,] GenerateNoiseMap(int width, int height, float scale, int octaves, float lacunarity, float dimension, int seed, Vector2 viewPos)
        {
            // 2D array to store noise values
            float[,] noiseMap = new float[width, height];

            // Nested loop through coordinates
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Generate a singular noise sample for this coordinate
                    noiseMap[x, y] = GenerateSimpleSample(new float3(x, y, 0), scale, seed,viewPos, octaves, lacunarity, dimension, false);
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
        public static float GenerateSimple2DNoiseValue(float x, float y, float scale, int octaves, float lacunarity, float dimension, int seed)
        {
            return GenerateSimpleSample(new float3(x, y, 0), scale, seed, Vector2.zero, octaves, lacunarity, dimension, false);
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
        public static float GenerateSimple3DNoiseValue(float x, float y, float z, float scale, int octaves, float lacunarity, float dimension, int seed)
        {
            return GenerateSimpleSample(new float3(x, y, z), scale, seed, Vector2.zero, octaves, lacunarity, dimension, true);
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
        private static float GenerateSimpleSample(float3 coords, float scale, int seed, Vector2 viewPos, int octaves, float lacunarity, float dimension, bool threeDimensions)
        {
            float noiseReturn = 0;

            for (int i = 0; i < octaves; i++)
            {
                var amplitude = (float) Math.Pow(dimension, i);
                var frequency = (float) Math.Pow(lacunarity, i);
                
                // Find the sample coordinates to use in the noise function
                float xSample = coords.x / scale * frequency;
                float ySample = coords.y / scale * frequency;

                // Specific dimension requirements
                if (threeDimensions)
                {
                    // Find z sample
                    float zSample = coords.z / scale * frequency;

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
            }

            return noiseReturn;
        }
        
        public static float GenerateRigid2DNoiseValue(float x, float y, float scale, int octaves, float lacunarity, float dimension, int seed)
        {
            return GenerateSimpleSample(new float3(x, y, 0), scale, seed, Vector2.zero, octaves, lacunarity, dimension, false);
        }
        
        public static float GenerateRigid3DNoiseValue(float x, float y, float z, float scale, int octaves, float lacunarity, float dimension, int seed)
        {
            return GenerateRigidSample(new float3(x, y, z), scale, seed, Vector2.zero, octaves, lacunarity, dimension, true);
        }

        private static float GenerateRigidSample(float3 coords, float scale, int seed, Vector2 viewPos, int octaves, float lacunarity, float dimension, bool threeDimensions)
        {
            float noiseReturn = 0;
            float weight = 1;
            
            for (int i = 0; i < octaves; i++)
            {
                var amplitude = (float) Math.Pow(dimension, i);
                var frequency = (float) Math.Pow(lacunarity, i);
                
                // Find the sample coordinates to use in the noise function
                float xSample = coords.x / scale * frequency;
                float ySample = coords.y / scale * frequency;

                // Specific dimension requirements
                if (threeDimensions)
                {
                    // Find z sample
                    float zSample = coords.z / scale * frequency;

                    var v = 1 - Math.Abs(FastNoiseLite.SingleOpenSimplex2S(seed + i, xSample, ySample, zSample));
                    v *= v;
                    v *= weight;
                    weight = v;

                    // Generate 3D noise value
                    noiseReturn += v * amplitude;
                }
                else
                {
                    // Add view position to x and y samples
                    xSample += viewPos.x;
                    ySample += viewPos.y;

                    var v = 1 - Math.Abs(FastNoiseLite.SingleOpenSimplex2S(seed + i, xSample, ySample));
                    v *= v;
                    v *= weight;
                    weight = v;

                    // Generate 2D noise value
                    noiseReturn += v * amplitude;

                }
            }

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
