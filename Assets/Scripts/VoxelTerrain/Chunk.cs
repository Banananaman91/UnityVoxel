using System;
using System.Collections;
using System.Collections.Generic;
using Noise;
using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Chunk : MonoBehaviour
{
    public const int ChunkSize = 16;
    private BlockType[,,] Voxels = new BlockType[ChunkSize,ChunkSize,ChunkSize];
    
    public MeshFilter MeshFilter => GetComponent<MeshFilter>();
    
    public BlockType this[int x, int y, int z]
    {
        get => Voxels[x, y, z];
        set => Voxels[x, y, z] = value;
    }

    public MeshCube MeshCube;

    public Chunk()
    {
        MeshCube = new MeshCube(this);
    }

    private void Awake()
    {
        float[][] perlinNoiseArray;
        perlinNoiseArray = new float[ChunkSize][];
        int octaveCount = 5;
        // Calculate the perlinNoiseArray
        perlinNoiseArray = PerlinNoise.GeneratePerlinNoise(ChunkSize,ChunkSize, octaveCount);
                
        int _curHeight;
        
        for(int i = 0; i < ChunkSize; i++)
        {
            for(int k = 0; k < ChunkSize; k++)
            {
                _curHeight = (int)(ChunkSize*Mathf.Clamp01(perlinNoiseArray[i][k]));
                for(int j = 0; j < ChunkSize; j++)
                {
                    if (j < _curHeight)
                    {
                        Voxels[i, j, k] = BlockType.Dirt;
                    }
                }
            }
        }
        var mesh = MeshCube.CreateMesh();
        MeshFilter.mesh = mesh;
    }
}
