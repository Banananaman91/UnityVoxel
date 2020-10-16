using System;
using MMesh;
using UnityEngine;

namespace VoxelTerrain
{
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
    }
}
