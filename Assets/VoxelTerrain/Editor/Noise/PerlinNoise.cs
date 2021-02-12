using UnityEngine;

//Credit to the creation of this script to Joseph Gallear
//While this package is created I have copied his code to be used here
//As when our projects merge I expect him to have his own file somewhere
//But I'm working separately on this as a package
namespace VoxelTerrain.Editor.Noise
{
    public class PerlinNoise : MonoBehaviour
    {
        /// <summary>
        /// Generates noise data using PRNG and multiple octaves
        /// </summary>
        /// <param name="width">Size of the map on the X axis</param>
        /// <param name="height">Size of the map on the Y axis</param>
        /// <param name="scale">Zoom level of the noise when displayed</param>
        /// <param name="seed">Seed of the generation</param>
        /// <param name="groundLevel">Ground level to limit lowest value</param>
        /// <param name="viewPos">Displayed position of the map</param>
        /// <returns>Noise values in 2D array</returns>
        public static float[,] GenerateNoise(int width, int height, float scale, int seed, float groundLevel, Vector2 viewPos)
        {
            // Mostly fixed variables for multiple octaves
            int octaves = 4; // Number of layers of noise
            float dimension = 0.5f; // Amplitude decrease each octave
            float lacunarity = 2; // Frequency increase each octave

            // 2D array to store noise values
            float[,] noiseMap = new float[width, height];

            // Create random number generator with seed
            System.Random numGen = new System.Random(seed);

            // Randomise a value for each octave so that the noise is different for each layer
            Vector2[] rngValues = new Vector2[octaves];
            for (int i = 0; i < rngValues.Length; i++)
            {
                rngValues[i] = new Vector2(numGen.Next(-100000, 100000), numGen.Next(-100000, 100000));
            }

            // Nested loop through coordinates
            for (int y = 0; y < height; y ++)
            {
                for (int x = 0; x < width; x++)
                {
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
                        noiseMap[x, y] += noiseValue * amplitude;

                        // Multiply the amplitude and frequency each octave
                        amplitude *= dimension;
                        frequency *= lacunarity;
                    }

                    // Set the value at the current coordinate and subtract ground level
                    noiseMap[x, y] -= groundLevel;

                    // Anything below ground level is moved up to 0 for flat land
                    if (noiseMap[x, y] < 0)
                    {
                        noiseMap[x, y] = 0;
                    }
                }
            }

            return noiseMap;
        }
    }
}
