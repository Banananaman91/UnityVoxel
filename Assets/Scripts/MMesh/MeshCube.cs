using System.Collections.Generic;
using UnityEngine;
using VoxelTerrain;

namespace MMesh
{
    public readonly struct MeshCube
    {
        private readonly Chunk _chunk;
    
        private static readonly List<Vector3> Vertices = new List<Vector3>();
        private static readonly List<int>Triangles = new List<int>();


        private static readonly Vector3[] CubeVertices = {
            new Vector3 (0, 0, 0), //0
            new Vector3 (1, 0, 0), //1
            new Vector3 (1, 1, 0), //2
            new Vector3 (0, 1, 0), //3
            new Vector3 (0, 1, 1), //4
            new Vector3 (1, 1, 1), //5
            new Vector3 (1, 0, 1), //6
            new Vector3 (0, 0, 1), //7
        }; //Vertices Cheat Sheet
        // Right Face 1 2 5 6
        // Left Face 7 4 3 0
        // Top Face 3 4 5 2
        // Bottom Face 0 1 6 7
        // Back Face 6 5 4 7
        // Front Face 0 3 2 1

        private static readonly int[] CubeTriangles = {
            // Front
            0, 2, 1,
            0, 3, 2,
            // Top
            2, 3, 4,
            2, 4, 5,
            // Right
            1, 2, 5,
            1, 5, 6,
            // Left
            0, 7, 4,
            0, 4, 3,
            // Back
            5, 4, 7,
            5, 7, 6,
            // Bottom
            0, 6, 7,
            0, 1, 6
        };

        public MeshCube(Chunk chunk)
        {
            _chunk = chunk;
        }

        public Mesh CreateMesh()
        {
            Vertices.Clear();
            Triangles.Clear();

            for (var x = 0; x < Chunk.ChunkSize; x++)
            {
                for (var y = 0; y < Chunk.ChunkSize; y++)
                {
                    for (var z = 0; z < Chunk.ChunkSize; z++)
                    {
                        var voxelType = _chunk[x, y, z];
                        // If it is air we ignore this block
                        if (voxelType == 0)
                            continue;
                        var pos = new Vector3(x, y, z);
                        // Remember current position in vertices list so we can add triangles relative to that
                        var verticesPos = Vertices.Count;
                        var numFaces = 0;

                        if (x == Chunk.ChunkSize - 1 || _chunk[x + 1, y, z] == 0) //right face
                        {
                            Vertices.Add(pos + CubeVertices[1]);
                            Vertices.Add(pos + CubeVertices[2]);
                            Vertices.Add(pos + CubeVertices[5]);
                            Vertices.Add(pos + CubeVertices[6]);
                            numFaces++;
                        }
                    
                        if (x == 0 || x > 0 && _chunk[x - 1, y, z] == 0) //left face
                        {
                            Vertices.Add(pos + CubeVertices[7]);
                            Vertices.Add(pos + CubeVertices[4]);
                            Vertices.Add(pos + CubeVertices[3]);
                            Vertices.Add(pos + CubeVertices[0]);
                            numFaces++;
                        }
                    
                        if (y == Chunk.ChunkSize - 1 || _chunk[x, y + 1, z] == 0) //top face
                        {
                            Vertices.Add(pos + CubeVertices[3]);
                            Vertices.Add(pos + CubeVertices[4]);
                            Vertices.Add(pos + CubeVertices[5]);
                            Vertices.Add(pos + CubeVertices[2]);
                            numFaces++;
                        }
                    
                        if (y == 0 || y > 0 && _chunk[x, y - 1, z] == 0) //bottom face
                        {
                            Vertices.Add(pos + CubeVertices[0]);
                            Vertices.Add(pos + CubeVertices[1]);
                            Vertices.Add(pos + CubeVertices[6]);
                            Vertices.Add(pos + CubeVertices[7]);
                            numFaces++;
                        }
                    
                        if (z == Chunk.ChunkSize - 1 || _chunk[x, y, z + 1] == 0) //back face
                        {
                            Vertices.Add(pos + CubeVertices[6]);
                            Vertices.Add(pos + CubeVertices[5]);
                            Vertices.Add(pos + CubeVertices[4]);
                            Vertices.Add(pos + CubeVertices[7]);
                            numFaces++;
                        }
                    
                        if (z == 0 || z > 0 && _chunk[x, y, z - 1] == 0) //front face
                        {
                            Vertices.Add(pos + CubeVertices[0]);
                            Vertices.Add(pos + CubeVertices[3]);
                            Vertices.Add(pos + CubeVertices[2]);
                            Vertices.Add(pos + CubeVertices[1]);
                            numFaces++;
                        }

                        var tl = Vertices.Count - 4 * numFaces;
                        for (var i = 0; i < numFaces; i++)
                        {
                            Triangles.AddRange(new [] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                        }
                    }
                }
            }
            // Apply new mesh to MeshFilter
            var mesh = new Mesh();
            mesh.SetVertices(Vertices);
            mesh.SetTriangles(Triangles.ToArray(), 0);
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}