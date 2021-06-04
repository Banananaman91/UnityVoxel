using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using VoxelTerrain.DataConversion;
using VoxelTerrain.MMesh;

namespace VoxelTerrain.Engine
{
    public class Chunk
    {
        public const int ChunkSize = 32; //Leave at this size
        public const int ChunkHeight = 256; //This should be 16 too, but I wanted taller chunks
        public Voxel[] Voxels;
        private VoxelEngine Engine;
        private GameObject Entity;
        public Vector3 Position;

        public ComputeBuffer pointBuffer;
        public ComputeBuffer triangleBuffer;
        public ComputeBuffer triCountBuffer;

        //Used to find voxel at position
        public Engine.Voxel this[float x, float y, float z]
        {
            get => Voxels[Converter.PosToIndex((int)x, (int)y, (int)z)];
            set => Voxels[Converter.PosToIndex((int)x, (int)y, (int)z)] = value;
        }

        //Add the game object this chunk is connected to
        public void AddEntity(GameObject entity) => Entity = entity;
        //Get the game object this object is connected to
        public GameObject GetEntity() => Entity ? Entity : null;
        //Add the engine
        public void AddEngine(VoxelEngine engine) => Engine = engine;
        //Set voxel at this position
        public void SetVoxel(Vector3 pos, VoxelType vox)
        {
            var voxel = this[(int) pos.x, (int) pos.y, (int) pos.z];
            voxel.Type = (byte) vox;
        }

        //Create the mesh data and set it to the object
        public void SetMesh(Vector3 origin, ComputeShader shader)
        {
            Position = origin;
            //If we don't have an entity then this isn't a chunk being used in the scene
            //So don't waste time making a mesh
            if (!Entity) return;
            
            var mesh = new Mesh();

            var monoGo = Entity.GetComponent<MonoChunk>();
            
            triangleBuffer.SetCounterValue(0);
            shader.SetBuffer(0, "points", pointBuffer);
            shader.SetBuffer(0, "triangles", triangleBuffer);
            shader.SetInt("width", ChunkSize);
            shader.SetInt("height", ChunkHeight);

            shader.Dispatch(0, (ChunkSize) / 8, (ChunkHeight) / 8, (ChunkSize) / 8);
            
            ComputeBuffer.CopyCount(triangleBuffer, triCountBuffer, 0);
            int[] triCountArray = { 0 };
            triCountBuffer.GetData(triCountArray);
            int numTris = triCountArray[0];

            Triangle[] tris = new Triangle[numTris];
            triangleBuffer.GetData(tris, 0, 0, numTris);

            var vertices = new Vector3[numTris * 3];
            var meshTriangles = new int[numTris * 3];

            for (int i = 0; i < numTris; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    meshTriangles[i * 3 + j] = i * 3 + j;
                    vertices[i * 3 + j] = tris[i][j];
                }
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(meshTriangles, 0);

            // #region NotJobs
            // var meshCreator = new MeshCreator(Engine.WorldData);
            //
            // //Build mesh data
            // meshCreator.SetMesh(Voxels, origin.x, origin.y, origin.z,
            //     Engine.ChunkInfo.VoxelSize, Engine.ChunkInfo.InterpolateMesh);
            //
            // //Update mesh
            // mesh.vertices = meshCreator.Vertices.ToArray();
            // mesh.triangles = meshCreator.Triangles.ToArray();            
            //
            mesh.SetUVs(0, new List<Vector2>(mesh.vertices.Length));
            // //Set uv channel to contain voxel uv data
            // mesh.SetUVs(1, meshCreator.uv0);
            // //Set uv channel to contain barycentric uv data
            // mesh.SetUVs(2, meshCreator.uv1);
            // #endregion
            
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            
            //Name the mesh, because we're not savages
            mesh.name = "Chunk: " + origin;

            monoGo.MeshFilter.sharedMesh = mesh;
            //monoGo.MeshCollider.sharedMesh = mesh;
        }

        //Constructor, for constructioning
        public Chunk(VoxelEngine engine)
        {
            Engine = engine;
            Voxels = new Voxel[ChunkSize * ChunkHeight * ChunkSize];
        }
        
        struct Triangle {
#pragma warning disable 649 // disable unassigned variable warning
            public Vector3 a;
            public Vector3 b;
            public Vector3 c;

            public Vector3 this [int i] {
                get {
                    switch (i) {
                        case 0:
                            return a;
                        case 1:
                            return b;
                        default:
                            return c;
                    }
                }
            }
        }
    }
}
