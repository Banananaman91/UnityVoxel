using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VoxelTerrain;

namespace MMesh
{
    public struct MeshCube
    {
        public readonly List<Vector3> Vertices;
        public readonly List<int> Triangles;
        public readonly List<Color> Colors;
        private readonly Chunk _chunk;
        private readonly VoxelEngine _engine;
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

        public MeshCube(Chunk chunk)
        {
            _chunk = chunk;
            _engine = chunk.Engine;
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

        public async void CreateMesh(float x, float y, float z)
        {
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            
            await GetMeshData(x, y, z);

            _chunk.MeshUpdate = true;
        }

        private async Task GetMeshData(float x, float y, float z)
        { 
            for (var i = 0; i < Chunk.ChunkSize; i++)
            {
                for (var j = 0; j < Chunk.ChunkHeight; j++)
                {
                    for (var k = 0; k < Chunk.ChunkSize; k++)
                    {
                        var voxelType = _chunk[i, j, k];
                        // If it is air we ignore this block
                        if (voxelType == 0)
                            continue;
                        _pos = new Vector3(i, j, k) * _engine.VoxelSize;
                        // Remember current position in vertices list so we can add triangles relative to that
                        _numFaces = 0;

                        //for each face, check corresponding position for potential voxel type
                        //works for spaces where voxels don't currently exist
                        //neighbour checks will be required once destruction/construction is added to voxel mechanics

                        #region RightFace

                        if (_chunk.SetVoxelType(x + ((i + 1) * _engine.VoxelSize), y + (j * _engine.VoxelSize), z + (k * _engine.VoxelSize)) == 0) //right face
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

                        #endregion

                        #region LeftFace


                        if (_chunk.SetVoxelType(x + ((i - 1) * _engine.VoxelSize), y + (j * _engine.VoxelSize), z + (k * _engine.VoxelSize)) == 0) //left face
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

                        #endregion

                        #region TopFace


                        if (_chunk.SetVoxelType(x + (i * _engine.VoxelSize), y + ((j + 1) * _engine.VoxelSize), z + (k * _engine.VoxelSize)) == 0) //top face
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

                        #endregion

                        #region BottomFace

                        if (_chunk.SetVoxelType(x + (i * _engine.VoxelSize), y + ((j - 1) * _engine.VoxelSize), z + (k * _engine.VoxelSize)) == 0) //bottom face
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

                        #endregion

                        #region BackFace

                        if (_chunk.SetVoxelType(x + (i * _engine.VoxelSize), y + (j * _engine.VoxelSize), z + ((k + 1) * _engine.VoxelSize)) == 0) //back face
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

                        #endregion

                        #region FrontFace

                        if (_chunk.SetVoxelType(x + (i * _engine.VoxelSize), y + (j * _engine.VoxelSize), z + ((k - 1) * _engine.VoxelSize)) == 0) //front face
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

                        #endregion

                        var tl = Vertices.Count - 4 * _numFaces;
                        for (var l = 0; l < _numFaces; l++)
                        {
                            Triangles.AddRange(new[]
                            {
                                tl + l * 4, tl + l * 4 + 1, tl + l * 4 + 2, tl + l * 4, tl + l * 4 + 2, tl + l * 4 + 3
                            });
                        }
                    }
                }
            }
        }
    }
}