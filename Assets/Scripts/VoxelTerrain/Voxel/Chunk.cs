using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.MMesh;
using VoxelTerrain.Voxel.Dependencies;
using VoxelTerrain.Voxel.Jobs;

namespace VoxelTerrain.Voxel
{
    public class Chunk
    {
        public const int ChunkSize = 16; //Leave at this size
        public const int ChunkHeight = 512; //This should be 16 too, but I wanted taller chunks
        public byte[] Voxels;
        private VoxelEngine Engine;
        private GameObject Entity;
        public Vector3 Position;

        //Used to find voxel at position
        public byte this[float x, float y, float z]
        {
            get => Voxels[PosToIndex((int)x, (int)y, (int)z)];
            set => Voxels[PosToIndex((int)x, (int)y, (int)z)] = value;
        }

        //Add the game object this chunk is connected to
        public void AddEntity(GameObject entity) => Entity = entity;
        //Get the game object this object is connected to
        public GameObject GetEntity() => Entity ? Entity : null;
        //Add the engine
        public void AddEngine(VoxelEngine engine) => Engine = engine;
        //Set voxel at this position
        public void SetVoxel(Vector3 pos, VoxelType vox) => this[(int) pos.x, (int) pos.y, (int) pos.z] = (byte) vox;
        //Create the mesh data and set it to the object
        public void SetMesh(Vector3 origin)
        {
            Position = origin;
            //If we don't have an entity then this isn't a chunk being used in the scene
            //So don't waste time making a mesh
            if (!Entity) return;
            
            var meshCreator = new MeshCreator(origin, Engine.ChunkInfo.VoxelSize, Engine.WorldData);

            //Build mesh data
            meshCreator.SetMesh(Voxels, origin.x, origin.y, origin.z,
                Engine.ChunkInfo.VoxelSize);
            
            //Jobs
            // var rightPos = new Vector3(origin.x + (ChunkSize * Engine.ChunkInfo.VoxelSize), origin.y, origin.z);
            // var forwardPos = new Vector3(origin.x, origin.y, origin.z + (ChunkSize * Engine.ChunkInfo.VoxelSize));
            // var forwardRight = new Vector3(rightPos.x, origin.y, forwardPos.z);
            //
            // var rightChunk = Engine.WorldData.GetNonNullChunkAt(rightPos);
            // var forwardChunk = Engine.WorldData.GetNonNullChunkAt(forwardPos);
            // var rightForwardChunk = Engine.WorldData.GetNonNullChunkAt(forwardRight);
            // //var currentChunk = Engine.WorldData.GetNonNullChunkAt(origin);
            //
            // NativeArray<byte> rVox = default;
            // NativeArray<byte> forVox = default;
            // NativeArray<byte> rForVox = default;
            //
            // if (rightChunk != null) rVox = new NativeArray<byte>(rightChunk.Voxels, Allocator.Persistent);
            // if (forwardChunk != null) forVox = new NativeArray<byte>(forwardChunk.Voxels, Allocator.Persistent);
            // if (rightForwardChunk != null)
            //     rForVox = new NativeArray<byte>(rightForwardChunk.Voxels, Allocator.Persistent);
            //
            // var meshJob = new MeshJob
            // {
            //     vertices = new NativeArray<Vector3>(0, Allocator.Persistent),
            //     triangles = new NativeArray<int>(0, Allocator.Persistent),
            //     uv0 = new NativeArray<Vector4>(0, Allocator.Persistent),
            //     uv1 = new NativeArray<Vector4>(0, Allocator.Persistent),
            //     currentVoxels = new NativeArray<byte>(Voxels, Allocator.Persistent),
            //     rightVoxels = rVox,
            //     forwardVoxels = forVox,
            //     rightForwardVoxels = rForVox,
            //     scale = Engine.ChunkInfo.VoxelSize,
            //     origin = origin,
            //     interpolate = true,
            //     noiseScale = Engine.NoiseScale,
            //     seed = Engine.WorldInfo.Seed,
            //     groundLevel = Engine.WorldInfo.GroundLevel
            // };
            // meshJob.Schedule().Complete();

            var monoGo = Entity.GetComponent<MonoChunk>();
            
            var mesh = new Mesh();
            //Update mesh
            mesh.vertices = meshCreator.Vertices.ToArray();
            mesh.triangles = meshCreator.Triangles.ToArray();            
            
            //Jobs
            // mesh.vertices = meshJob.vertices.ToArray();
            // meshJob.vertices.Dispose();
            // mesh.triangles = meshJob.triangles.ToArray();
            // meshJob.triangles.Dispose();
            
            mesh.SetUVs(0, new List<Vector2>(mesh.vertices.Length));
            //Set uv channel to contain voxel uv data
            mesh.SetUVs(1, meshCreator.uv0);
            //Set uv channel to contain barycentric uv data
            mesh.SetUVs(2, meshCreator.uv1);
            
            //Jobs
            // mesh.SetUVs(0, new List<Vector2>(mesh.vertices.Length));
            // //Set uv channel to contain voxel uv data
            // mesh.SetUVs(1, meshJob.uv0);
            // meshJob.uv0.Dispose();
            // //Set uv channel to contain barycentric uv data
            // mesh.SetUVs(2, meshJob.uv1);
            // meshJob.uv1.Dispose();
            //
            // rVox.Dispose();
            // forVox.Dispose();
            // rForVox.Dispose();
            //
            // meshJob.currentVoxels.Dispose();
            // meshJob.rightVoxels.Dispose();
            // meshJob.rightForwardVoxels.Dispose();
            // meshJob.forwardVoxels.Dispose();
            
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            //Name the mesh, because we're not savages
            mesh.name = "Chunk: " + origin;

            monoGo.MeshFilter.sharedMesh = mesh;
            monoGo.MeshCollider.sharedMesh = mesh;
        }
        
        //convert world position to index position of the voxel array
        public static int PosToIndex(int x, int y, int z) => z * (ChunkSize) * (ChunkHeight) + y * (ChunkSize) + x;

        //Constructor, for constructioning
        public Chunk(VoxelEngine engine)
        {
            Engine = engine;
            Voxels = new byte[ChunkSize * ChunkHeight * ChunkSize];
        }
    }
}
