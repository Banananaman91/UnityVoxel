using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VoxelTerrain;

namespace MMesh
{
    public struct MeshCube
    {
        private readonly Chunk _chunk;
        private readonly VoxelEngine _engine;
    
        public readonly List<Vector3> Vertices;
        public readonly List<int> Triangles;
        public readonly List<Color> Colors;
        private static readonly Color32[] _colors = {new Color32(66, 177, 0, 255), new Color32(87, 51, 0, 255), new Color32(85, 85, 85, 255), new Color32(255, 176, 0, 255), new Color32(255, 255, 255, 255)   };
        private Vector3 _pos;
        private int _numFaces;

        private readonly Vector3[] CubeVertices;

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

        private Mesh mesh;

        public MeshCube(Chunk chunk)
        {
            _chunk = chunk;
            _engine = chunk.Engine;
            mesh = new Mesh();
            CubeVertices = new Vector3[] {
            new Vector3 (0, 0, 0), //0
            new Vector3 (1 * _engine.VoxelSize, 0, 0), //1
            new Vector3 (1 * _engine.VoxelSize, 1 * _engine.VoxelSize, 0), //2
            new Vector3 (0, 1 * _engine.VoxelSize, 0), //3
            new Vector3 (0, 1 * _engine.VoxelSize, 1 * _engine.VoxelSize), //4
            new Vector3 (1 * _engine.VoxelSize, 1 * _engine.VoxelSize, 1 * _engine.VoxelSize), //5
            new Vector3 (1 * _engine.VoxelSize, 0, 1 * _engine.VoxelSize), //6
            new Vector3 (0, 0, 1 * _engine.VoxelSize), //7
        }; //Vertices Cheat Sheet
        // Right Face 1 2 5 6
        // Left Face 7 4 3 0
        // Top Face 3 4 5 2
        // Bottom Face 0 1 6 7
        // Back Face 6 5 4 7
        // Front Face 0 3 2 1

        Vertices = new List<Vector3>();
        Triangles = new List<int>();
        Colors = new List<Color>();
        _pos = new Vector3(0, 0, 0);
        _numFaces = 0;
        }

        public async void CreateMesh()
        {
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            
            await GetMeshData();

            // Apply new mesh to MeshFilter
            // mesh = new Mesh();
            // mesh.SetVertices(Vertices);
            // mesh.SetTriangles(Triangles.ToArray(), 0);
            // mesh.SetColors(Colors);
            // mesh.RecalculateNormals();
            // _chunk.MeshFilter.mesh = mesh;
            _chunk.MeshUpdate = true;
        }

        private async Task GetMeshData()
        { 
            for (var x = 0; x < Chunk.ChunkSize; x++)
            {
                for (var y = 0; y < Chunk.ChunkHeight; y++)
                {
                    for (var z = 0; z < Chunk.ChunkSize; z++)
                    {
                        var voxelType = _chunk[x, y, z];
                        // If it is air we ignore this block
                        if (voxelType == 0)
                            continue;
                        _pos = new Vector3(x, y, z) * _engine.VoxelSize;
                        // Remember current position in vertices list so we can add triangles relative to that
                        _numFaces = 0;

                        if (x == Chunk.ChunkSize - 1 || _chunk[x + 1, y, z] == 0) //right face
                        {
                            Vertices.Add(_pos + CubeVertices[1]);
                            Vertices.Add(_pos + CubeVertices[2]);
                            Vertices.Add(_pos + CubeVertices[5]);
                            Vertices.Add(_pos + CubeVertices[6]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            _numFaces++;
                        }
                    
                        if (x == 0 || x > 0 && _chunk[x - 1, y, z] == 0) //left face
                        {
                            Vertices.Add(_pos + CubeVertices[7]);
                            Vertices.Add(_pos + CubeVertices[4]);
                            Vertices.Add(_pos + CubeVertices[3]);
                            Vertices.Add(_pos + CubeVertices[0]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            _numFaces++;
                        }
                    
                        if (y == Chunk.ChunkHeight - 1 || _chunk[x, y + 1, z] == 0) //top face
                        {
                            Vertices.Add(_pos + CubeVertices[3]);
                            Vertices.Add(_pos + CubeVertices[4]);
                            Vertices.Add(_pos + CubeVertices[5]);
                            Vertices.Add(_pos + CubeVertices[2]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            _numFaces++;
                        }
                    
                        if (y == 0 || y > 0 && _chunk[x, y - 1, z] == 0) //bottom face
                        {
                            Vertices.Add(_pos + CubeVertices[0]);
                            Vertices.Add(_pos + CubeVertices[1]);
                            Vertices.Add(_pos + CubeVertices[6]);
                            Vertices.Add(_pos + CubeVertices[7]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            _numFaces++;
                        }
                    
                        if (z == Chunk.ChunkSize - 1 || _chunk[x, y, z + 1] == 0) //back face
                        {
                            Vertices.Add(_pos + CubeVertices[6]);
                            Vertices.Add(_pos + CubeVertices[5]);
                            Vertices.Add(_pos + CubeVertices[4]);
                            Vertices.Add(_pos + CubeVertices[7]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            _numFaces++;
                        }
                    
                        if (z == 0 || z > 0 && _chunk[x, y, z - 1] == 0) //front face
                        {
                            Vertices.Add(_pos + CubeVertices[0]);
                            Vertices.Add(_pos + CubeVertices[3]);
                            Vertices.Add(_pos + CubeVertices[2]);
                            Vertices.Add(_pos + CubeVertices[1]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            Colors.Add(_colors[(int) (voxelType - 1)]);
                            _numFaces++;
                        }

                        var tl = Vertices.Count - 4 * _numFaces;
                        for (var i = 0; i < _numFaces; i++)
                        {
                            Triangles.AddRange(new [] { tl + i * 4, tl + i * 4 + 1, tl + i * 4 + 2, tl + i * 4, tl + i * 4 + 2, tl + i * 4 + 3 });
                        }
                    }
                }
            }
        }
    }
}