using UnityEngine;

//Credit to the creation of this script to Joseph Gallear
//While this package is created I have copied his code to be used here
//As when our projects merge I expect him to have his own file somewhere
//But I'm working separately on this as a package
namespace VoxelTerrain.Editor.Noise
{
    public class PerlinNoise : MonoBehaviour
    {

        // Mostly fixed variables for multiple octaves
        private static int octaves = 4; // Number of layers of noise
        private static float dimension = 0.5f; // Amplitude decrease each octave
        private static float lacunarity = 2; // Frequency increase each octave

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
        public static float[,] GenerateNoiseMap(int width, int height, float scale, int seed, float groundLevel,
            Vector2 viewPos)
        {
            // 2D array to store noise values
            float[,] noiseMap = new float[width, height];

            // Randomise a value for each octave so that the noise is different for each layer
            Vector3[] rngValues = GenerateSeed(seed);

            // Nested loop through coordinates
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Generate a singular noise sample for this coordinate
                    noiseMap[x, y] = Generate2DSample(x, y, scale, groundLevel, viewPos, rngValues);
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
        public static float Generate2DNoiseValue(float x, float y, float scale, int seed, float groundLevel)
        {
            return Generate2DSample(x, y, scale, groundLevel, new Vector2(0, 0), GenerateSeed(seed));
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
        public static float Generate3DNoiseValue(float x, float y, float z, float scale, int seed)
        {
            return Generate3DSample(x, y, z, scale, GenerateSeed(seed));
        }

        /// <summary>
        /// Generates a singular 2D noise sample using PRNG and multiple octaves
        /// </summary>
        /// <param name="x">X coordinate of sample</param>
        /// <param name="y">Y coordinate of sample</param>
        /// <param name="scale">Zoom level of the noise when displayed</param>
        /// <param name="groundLevel">Ground level to limit lowest value</param>
        /// <param name="viewPos">Displayed position of the map</param>
        /// <param name="rngValues">Seeded PRNG values</param>
        /// <returns>Singular 2D noise sample</returns>
        private static float Generate2DSample(float x, float y, float scale, float groundLevel, Vector2 viewPos,
            Vector3[] rngValues)
        {
            float noiseReturn = 0;

            // Local variables per coordinate
            float amplitude = 1; // Vertical scale of noise
            float frequency = 1; // Horizontal scale of noise

            for (int i = 0; i < octaves; i++)
            {
                // Find the sample coordinates to use in the noise function
                float xSample = x / scale * frequency + rngValues[i].x + viewPos.x;
                float ySample = y / scale * frequency + rngValues[i].y + viewPos.y;

                // Generate noise value from Perlin noise function and make it between -1 and 1
                float noiseValue = Mathf.PerlinNoise(xSample, ySample);

                // Exaggerate the noise value based on amplitude
                noiseReturn += noiseValue * amplitude;

                // Multiply the amplitude and frequency each octave
                amplitude *= dimension;
                frequency *= lacunarity;
            }

            // Set the value at the current coordinate and subtract ground level
            noiseReturn -= groundLevel;

            // Anything below ground level is moved up to 0 for flat land
            if (noiseReturn < 0)
            {
                noiseReturn = 0;
            }

            return noiseReturn * scale;
        }

        /// <summary>
        /// Generates a singular 3D noise sample using PRNG and multiple octaves
        /// </summary>
        /// <param name="x">X coordinate of sample</param>
        /// <param name="y">Y coordinate of sample</param>
        /// <param name="z">Z coordinate of sample</param>
        /// <param name="scale">Zoom level of the noise when displayed</param>
        /// <param name="rngValues">Seeded PRNG values</param>
        /// <returns>Singular noise sample</returns>
        private static float Generate3DSample(float x, float y, float z, float scale, Vector3[] rngValues)
        {
            float noiseReturn = 0;

            // Local variables per coordinate
            float amplitude = 1; // Vertical scale of noise
            float frequency = 1; // Horizontal scale of noise

            for (int i = 0; i < octaves; i++)
            {
                // Find the sample coordinates to use in the noise function
                float xSample = x / scale * frequency + rngValues[i].x;
                float ySample = y / scale * frequency + rngValues[i].y;
                float zSample = z / scale * frequency + rngValues[i].z;

                // Generate noise value from Perlin noise function and make it between -1 and 1
                float noiseValue = PerlinNoise3D(xSample, ySample, zSample);

                // Exaggerate the noise value based on amplitude
                noiseReturn += noiseValue * amplitude;

                // Multiply the amplitude and frequency each octave
                amplitude *= dimension;
                frequency *= lacunarity;
            }

            return noiseReturn * scale;
        }

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

        /// <summary>
        /// Generates random values from a given seed
        /// </summary>
        /// <param name="seed">Seed of the generation</param>
        /// <returns>Array of random values</returns>
        private static Vector3[] GenerateSeed(int seed)
        {
            // Create random number generator with seed
            System.Random numGen = new System.Random(seed);

            // Randomise a value for each octave so that the noise is different for each layer
            Vector3[] rngValues = new Vector3[octaves];
            for (int i = 0; i < rngValues.Length; i++)
            {
                rngValues[i] = new Vector3(numGen.Next(-100000, 100000), numGen.Next(-100000, 100000),
                    numGen.Next(-100000, 100000));
            }

            return rngValues;
        }
    }
}
