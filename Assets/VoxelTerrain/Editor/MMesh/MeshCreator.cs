using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using VoxelTerrain;
using VoxelTerrain.Editor.Voxel;
using VoxelTerrain.Editor.Voxel.Dependencies;

namespace MMesh
{
    public struct MeshCreator
    {
        public readonly List<Vector3> Vertices;
        public readonly List<int> Triangles;
        public readonly List<Color> Colors;
        private static readonly Color32[] _colors = {new Color32(66, 177, 0, 255), new Color32(87, 51, 0, 255), new Color32(85, 85, 85, 255), new Color32(255, 176, 0, 255), new Color32(255, 255, 255, 255), new Color32(0, 0, 255, 255), new Color32(110, 70, 0, 255)  };
        private Vector3 _pos;
        private int _numFaces;
        private readonly Vector3[] CubeVertices;
        private World _world;

        public MeshCreator(Vector3 pos, float size, World world)
        {
            _pos = pos;
            _world = world;
            Vertices = new List<Vector3>();
            Triangles = new List<int>();
            Colors = new List<Color>();
            _numFaces = 0;
            
            CubeVertices = new Vector3[] {
                new Vector3 (0, 0, 0), //0
                new Vector3 (1 * size, 0, 0), //1
                new Vector3 (1 * size, 1 * size, 0), //2
                new Vector3 (0, 1 * size, 0), //3
                new Vector3 (0, 1 * size, 1 * size), //4
                new Vector3 (1 * size, 1 * size, 1 * size), //5
                new Vector3 (1 * size, 0, 1 * size), //6
                new Vector3 (0, 0, 1 * size), //7
            }; //Vertices Cheat Sheet
            // Right Face 1 2 5 6
            // Left Face 7 4 3 0
            // Top Face 3 4 5 2
            // Bottom Face 0 1 6 7
            // Back Face 6 5 4 7
            // Front Face 0 3 2 1
        }
        // private static readonly int[] CubeTriangles = {
        //     // Front
        //     0, 2, 1,
        //     0, 3, 2,
        //     // Top
        //     2, 3, 4,
        //     2, 4, 5,
        //     // Right
        //     1, 2, 5,
        //     1, 5, 6,
        //     // Left
        //     0, 7, 4,
        //     0, 4, 3,
        //     // Back
        //     5, 4, 7,
        //     5, 7, 6,
        //     // Bottom
        //     0, 6, 7,
        //     0, 1, 6
        // };

        public async void SetMesh(float[] Voxels, float x, float y, float z, float size)
        {
            //MarchingCubes( Voxels, 16, 1, new Vector3(x, y, z), true);
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            
            await SetMeshData(Voxels, x, y, z, size);
            
            //_chunk.CreateMesh();
        }

        private void MarchingCubes(float[]voxels, int size, float scale, Vector3 origin, bool interpolate)
        {
            int flagIndex = 0;
            int index = 0;

            Vertices.Clear();
            Triangles.Clear();

            float[] afCubes = new float[8];

            for (int x = 0; x < Chunk.ChunkSize - 1; x++)
            {
                for (int y = 0; y < Chunk.ChunkHeight - 1; y++)
                {
                    for (int z = 0; z < Chunk.ChunkSize - 1; z++)
                    {
                        //Offsets are same as cornerOffsets[8]
                        afCubes[0] = voxels[Chunk.PosToIndex(x, y, z)];
                        afCubes[1] = voxels[Chunk.PosToIndex(x + 1, y, z)];
                        afCubes[2] = voxels[Chunk.PosToIndex(x + 1, y + 1, z)];
                        afCubes[3] = voxels[Chunk.PosToIndex(x, y + 1, z)];
                        afCubes[4] = voxels[Chunk.PosToIndex(x, y, z + 1)];
                        afCubes[5] = voxels[Chunk.PosToIndex(x + 1, y, z + 1)];
                        afCubes[6] = voxels[Chunk.PosToIndex(x + 1, y + 1, z + 1)];
                        afCubes[7] = voxels[Chunk.PosToIndex(x, y + 1, z + 1)];


                        //Calculate the index of the current cube configuration as follows:
                        //Loop over each of the 8 corners of the cube, and set the corresponding
                        //bit to 1 if its value is below the surface level.
                        //this will result in a value between 0 and 255
                        for (int vtest = 0; vtest < 8; vtest++)
                        {
                            if (afCubes[vtest] <= 0.0f)
                                flagIndex |= 1 << vtest;
                        }

                        //Skip to next if all corners are the same
                        if (flagIndex == 0x00 || flagIndex == 0xFF)
                            continue;

                        //Get the offset of this current block
                        var offset = new Vector3(x * scale, y * scale, z * scale);

                        for (int triangle = 0; triangle < 5; triangle++)
                        {
                            int edgeIndex = VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle];

                            if (edgeIndex < 0)
                                continue; //Skip if the edgeIndex is -1

                            for (int triangleCorner = 0; triangleCorner < 3; triangleCorner++)
                            {
                                edgeIndex =
                                    VoxelLookUp.a2iTriangleConnectionTable[flagIndex][3 * triangle + triangleCorner];

                                var edge1 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 0];
                                var edge2 = VoxelLookUp.edgeVertexOffsets[edgeIndex, 1];

                                edge1 *= scale;
                                edge2 *= scale;

                                Vector3 middle;
                                if (interpolate)
                                {
                                    float ofst;
                                    float s1 = voxels[Chunk.PosToIndex(x + (int) edge1.x, y + (int) edge1.y, z + (int) edge1.z)];
                                    float delta = s1 - voxels[Chunk.PosToIndex(x + (int) edge2.x, y + (int) edge2.y, z + (int) edge2.z)];
                                    if (delta == 0.0f)
                                        ofst = 0.5f;
                                    else
                                        ofst = s1 / delta;
                                    middle = edge1 + ofst * (edge2 - edge1);
                                }
                                else
                                {
                                    middle = (edge1 + edge2) * 0.5f;
                                }

                                Vertices.Add(offset + middle);
                                Triangles.Add(index++);
                            }
                        }
                    }
                }
            }
        }

        private async Task SetMeshData(float[] voxels, float x, float y, float z, float size)
        {
            for (var i = 0; i < Chunk.ChunkSize; i++)
            {
                for (var j = 0; j < Chunk.ChunkHeight; j++)
                {
                    for (var k = 0; k < Chunk.ChunkSize; k++)
                    {
                        var voxelType = voxels[Chunk.PosToIndex(i, j, k)];
                        // If it is air we ignore this block
                        if (voxelType == 0)
                            continue;
                        _pos = new Vector3(i, j, k) * size;
                        // Remember current position in vertices list so we can add triangles relative to that
                        _numFaces = 0;

                        //for each face, check corresponding position for potential voxel type
                        //works for spaces where voxels don't currently exist
                        //neighbour checks will be required once destruction/construction is added to voxel mechanics

                        float vox;
                        Chunk chunk;
                        
                        #region RightFace

                        if (i + 1 >= Chunk.ChunkSize)
                        {
                            chunk = _world.GetChunkAt(new Vector3(x + Chunk.ChunkSize, y, z));
                            if (chunk != null) vox = chunk[0, j, k];
                            else vox = 1;
                        }
                            
                        else vox = voxels[Chunk.PosToIndex((i + 1), j, k)];
                        
                        if (vox == 0) //right face
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

                        if (i - 1 < 0)
                        {
                            chunk = _world.GetChunkAt(new Vector3(x - Chunk.ChunkSize, y, z));
                            if (chunk != null) vox = chunk[Chunk.ChunkSize - 1, j, k];
                            else vox = 1;
                        }
                            
                        else 
                        {
                            vox = voxels[Chunk.PosToIndex(i - 1, j, k)];
                        }

                        if (vox == 0) //left face
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

                        if (j + 1 >= Chunk.ChunkHeight)
                            vox = 1;
                        else vox = voxels[Chunk.PosToIndex(i, j + 1, k)];

                        if (vox == 0) //top face
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
                        
                        if (j - 1 < 0)
                            vox = 1;
                        else vox = voxels[Chunk.PosToIndex(i, j - 1, k)];

                        if (vox == 0) //bottom face
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

                        if (k + 1 >= Chunk.ChunkSize)
                        {
                            chunk = _world.GetChunkAt(new Vector3(x, y, z + Chunk.ChunkSize));
                            if (chunk != null) vox = chunk[i, j, 0];
                            else vox = 1;
                        }
                            
                        else 
                        {
                            vox = voxels[Chunk.PosToIndex( i, j, k + 1)];
                        }

                        if (vox == 0) //back face
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

                        if (k - 1 < 0)
                        {
                            chunk = _world.GetChunkAt(new Vector3(x, y, z - Chunk.ChunkSize));
                            if (chunk != null) vox = chunk[i, j, Chunk.ChunkSize - 1];
                            else vox = 1;
                        }
                            
                        else
                        {
                            vox = voxels[Chunk.PosToIndex(i, j, k - 1)];
                        }

                        if (vox == 0) //front face
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