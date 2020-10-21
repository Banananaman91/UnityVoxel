﻿using System;
using MMesh;
using UnityEngine;

namespace VoxelTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        public const int ChunkSize = 16;
        public const int ChunkHeight = 32;
        private BlockType[,,] Voxels = new BlockType[ChunkSize,ChunkHeight,ChunkSize];
    
        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        
        public bool IsAvailable { get; set; }

        public BlockType this[int x, int y, int z]
        {
            get => Voxels[x, y, z];
            set => Voxels[x, y, z] = value;
        }

        public MeshCube MeshCube;

        public void Awake()
        {
            MeshCube = new MeshCube(this);
        }
    }
}
