using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGeneration : MonoBehaviour
{

    public float[,] GenerateNoiseMap(int mapDepth, int mapWidth, float scale,
        float offsetX, float offsetZ, Wave[] waves)
    {
        float[,] noiseMap = new float[mapDepth, mapWidth];

        for (int zIndex = 0; zIndex < mapDepth; zIndex++)
        {
            for (int xIndex = 0; xIndex < mapWidth; xIndex++)
            {
                float sampleX = (xIndex + offsetX) / scale;
                float sampleZ = (zIndex + offsetZ) / scale;

                float noise = 0f;
                float normalization = 0f;

                foreach (Wave wave in waves)
                {
                    //generate noise value using Perlin noise
                    noise += wave.amplitude * Mathf.PerlinNoise(sampleX * wave.frequency +
                        wave.seed, sampleZ * wave.frequency + wave.seed);
                    normalization += wave.amplitude;
                }

                //normalize noise value so it is between 0 and 1
                noise /= normalization;
                noiseMap[zIndex, xIndex] = noise;
            }
        }

        return noiseMap;
    }
}

[System.Serializable]
public class Wave
{
    public float seed;
    public float frequency;
    public float amplitude;
}
