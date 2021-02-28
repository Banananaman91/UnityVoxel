using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using MMesh;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using VoxelTerrain.Dependencies;
using VoxelTerrain.Editor.Noise;
using VoxelTerrain.Editor.Voxel.Jobs;
using Color = UnityEngine.Color;

namespace VoxelTerrain.Editor.Voxel
{
    public class Chunk
    {
        public const int ChunkSize = 16; //Leave at this size
        public const int ChunkHeight = 64; //This should be 16 too, but I wanted taller chunks
        public float[] Voxels;
        private VoxelEngine Engine;
        private string ChunkName;

        private GameObject Entity;

        //Used to find voxel at position
        public float this[float x, float y, float z]
        {
            get => Voxels[PosToIndex((int)x, (int)y, (int)z)];
            set => Voxels[PosToIndex((int)x, (int)y, (int)z)] = value;
        }

        public void AddEntity(GameObject entity) => Entity = entity;

        public void SetVoxel(Vector3 pos, VoxelType vox) => this[(int) pos.x, (int) pos.y, (int) pos.z] = (float) vox;

        public void VoxelsFromJob(ChunkVoxelSetter job)
        {
            Voxels = job.voxels.ToArray();
            job.voxels.Dispose();
        }

        public void SetMesh( Vector3 origin)
        {
            var meshCreator = new MeshCreator(origin, Engine.ChunkInfo.VoxelSize, Engine.WorldData);

            meshCreator.SetMesh(Voxels, origin.x, origin.y, origin.z,
                Engine.ChunkInfo.VoxelSize);

            var monoGo = Entity.GetComponent<MonoChunk>();
            
            var mesh = new Mesh();
            //Update mesh
            mesh.vertices = meshCreator.Vertices.ToArray();
            mesh.triangles = meshCreator.Triangles.ToArray();
            mesh.uv = new Vector2[mesh.vertices.Length];
            mesh.SetColors(meshCreator.Colors);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            mesh.name = ChunkName;

            monoGo.MeshFilter.sharedMesh = mesh;
            monoGo.MeshCollider.sharedMesh = mesh;
        }

        public static int PosToIndex(int x, int y, int z) => z * (ChunkSize) * (ChunkHeight) + y * (ChunkSize) + x;

        public Chunk(float x, float y, float z, float size, VoxelEngine engine)
        {
            Engine = engine;
            Voxels = new float[ChunkSize * ChunkHeight * ChunkSize];
            ChunkName = "Chunk: " + x + ", " + y + ", " + z;
        }
    }
}
