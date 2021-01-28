using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MMesh;
using UnityEngine;

namespace VoxelTerrain.Editor.Voxel
{
    
    public struct Chunk
    {
        public const int ChunkSize = 16; //Leave at this size
        public const int ChunkHeight = 32; //This should be 16 too, but I wanted taller chunks
        private BlockType[,,] Voxels;
        public Mesh mesh;
        //private MeshCube MeshCube;
        public VoxelEngine Engine;

        //Used to find voxel at position
        public BlockType this[int x, int y, int z]
        {
            get => Voxels[x, y, z];
            set => Voxels[x, y, z] = value;
        }

        public Chunk(float x, float y, float z, float size, VoxelEngine engine)
        {
            mesh = new Mesh();
            Engine = engine;
            Voxels = new BlockType[ChunkSize,ChunkHeight,ChunkSize];
            //MeshCube = new MeshCube(this);
            //SetVoxel(x, y, z, size);
            
            CubeVertices = new Vector3[] {
                new Vector3 (0, 0, 0), //0
                new Vector3 (1 * Engine.VoxelSize, 0, 0), //1
                new Vector3 (1 * Engine.VoxelSize, 1 * Engine.VoxelSize, 0), //2
                new Vector3 (0, 1 * Engine.VoxelSize, 0), //3
                new Vector3 (0, 1 * Engine.VoxelSize, 1 * Engine.VoxelSize), //4
                new Vector3 (1 * Engine.VoxelSize, 1 * Engine.VoxelSize, 1 * Engine.VoxelSize), //5
                new Vector3 (1 * Engine.VoxelSize, 0, 1 * Engine.VoxelSize), //6
                new Vector3 (0, 0, 1 * Engine.VoxelSize), //7
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

        public void CreateMesh()
        {
            //Update mesh
            mesh.SetVertices(Vertices);
            mesh.SetTriangles(Triangles.ToArray(), 0);
            mesh.SetColors(Colors);
            mesh.RecalculateNormals();
        }

        //Iterate through all voxels and set their type
        public void SetVoxel(float x, float y, float z, float size)
        {
            for(var i = 0; i < ChunkSize; i++)
            {
                for(var k = 0; k < ChunkSize; k++)
                {
                    for(var j = 0; j < ChunkHeight; j++)
                    {
                        this[i, j, k] = SetVoxelType(x + (i * size), y + (j * size), z + (k * size));
                    }
                }
            }
            //MeshUpdate = true;
            CreateMesh(x, y, z);
        }
        
        //set individual voxel type using noise function
        public BlockType SetVoxelType(float x, float y, float z)
        {
            //3D noise for heightmap
            var simplex1 = Engine._fastNoise.GetNoise(x * 0.3f, z * 0.3f) * Engine.SimplexOneScale;
            var simplex2 = Engine._fastNoise.GetNoise(x * 0.8f, z * 0.8f) * Engine.SimplexTwoScale;

            //3d noise for caves and overhangs and such
            var caveNoise1 = Engine._fastNoise.GetNoise(x * 0.3f, y * 0.3f, z * 0.3f) * Engine.CaveNoiseOneScale;
            var caveNoise2 = Engine._fastNoise.GetNoise(x * 0.8f, y * 0.8f, z * 0.8f) * Engine.CaveNoiseTwoScale;
            var caveMask = Engine._fastNoise.GetNoise(x, z) + Engine.CaveMask;
            
            //stone layer heightmap
            var simplexStone1 = Engine._fastNoise.GetNoise(x * 0.3f, z * 0.3f) * Engine.SimplexStoneOneScale;
            var simplexStone2 = Engine._fastNoise.GetNoise(x * 0.8f, z * 0.8f) * Engine.SimplexStoneTwoScale;

            var treeNoise1 = Engine._fastNoise.GetNoise(x * 0.5f, z * 0.5f) * 5;
            var treeNoise2 = Engine._fastNoise.GetNoise(x * 0.9f, z * 0.9f) * 50;
            var treeMask = Engine._fastNoise.GetNoise(x, z) + 8;
            
            var heightMap = simplex1 + simplex2;
            var caveMap = caveNoise1 + caveNoise2;
            var baseLandHeight = heightMap;
            var stoneHeightMap = simplexStone1 + simplexStone2;
            var baseStoneHeight = ChunkSize + stoneHeightMap;
            var treeMap = treeNoise1 + treeNoise2;

            var blockType = BlockType.Default;

            //under the surface, dirt block
            if(y <= baseLandHeight)
            {
                blockType = BlockType.Dirt;

                //just on the surface, use a grass type
                if (y > baseLandHeight - 1)
                {
                    if (treeMap > Mathf.Max(treeMask, 0.2f)) blockType = BlockType.Wood;
                    else blockType = BlockType.Grass;
                }

                //surface is above snow height, use snow type
                if (y > Engine.SnowHeight) blockType = BlockType.Snow;

                //too low for dirt, make it stone
                if(y <= baseStoneHeight && y < baseLandHeight - Engine.StoneDepth) blockType = BlockType.Stone;
            }

            //mask for generating caves
            
            if(caveMap > Mathf.Max(caveMask, .2f) && (y <= Engine.CaveStartHeight || y < baseLandHeight - -Engine.CaveStartHeight))
               blockType = BlockType.Default;

            return blockType;
        }
        
        public readonly List<Vector3> Vertices;
        public readonly List<int> Triangles;
        public readonly List<Color> Colors;
        private static readonly Color32[] _colors = {new Color32(66, 177, 0, 255), new Color32(87, 51, 0, 255), new Color32(85, 85, 85, 255), new Color32(255, 176, 0, 255), new Color32(255, 255, 255, 255), new Color32(0, 0, 255, 255), new Color32(110, 70, 0, 255)  };
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

        public async void CreateMesh(float x, float y, float z)
        {
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            
            await GetMeshData(x, y, z);
            
            //_chunk.CreateMesh();
        }

        private async Task GetMeshData(float x, float y, float z)
        {
            for (var i = 0; i < Chunk.ChunkSize; i++)
            {
                for (var j = 0; j < Chunk.ChunkHeight; j++)
                {
                    for (var k = 0; k < Chunk.ChunkSize; k++)
                    {
                        var voxelType = this[i, j, k];
                        // If it is air we ignore this block
                        if (voxelType == 0)
                            continue;
                        _pos = new Vector3(i, j, k) * Engine.VoxelSize;
                        // Remember current position in vertices list so we can add triangles relative to that
                        _numFaces = 0;

                        //for each face, check corresponding position for potential voxel type
                        //works for spaces where voxels don't currently exist
                        //neighbour checks will be required once destruction/construction is added to voxel mechanics

                        #region RightFace

                        if (SetVoxelType(x + ((i + 1) * Engine.VoxelSize), y + (j * Engine.VoxelSize),
                            z + (k * Engine.VoxelSize)) == 0) //right face
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


                        if (SetVoxelType(x + ((i - 1) * Engine.VoxelSize), y + (j * Engine.VoxelSize),
                            z + (k * Engine.VoxelSize)) == 0) //left face
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


                        if (SetVoxelType(x + (i * Engine.VoxelSize), y + ((j + 1) * Engine.VoxelSize),
                            z + (k * Engine.VoxelSize)) == 0) //top face
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

                        if (SetVoxelType(x + (i * Engine.VoxelSize), y + ((j - 1) * Engine.VoxelSize),
                            z + (k * Engine.VoxelSize)) == 0) //bottom face
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

                        if (SetVoxelType(x + (i * Engine.VoxelSize), y + (j * Engine.VoxelSize),
                            z + ((k + 1) * Engine.VoxelSize)) == 0) //back face
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

                        if (SetVoxelType(x + (i * Engine.VoxelSize), y + (j * Engine.VoxelSize),
                            z + ((k - 1) * Engine.VoxelSize)) == 0) //front face
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
        
        #region Equality Members

        public bool Equals(Chunk other)
        {
            return base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Chunk other && Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Chunk left, Chunk right)
        {
            return !left.Equals(right);
        }

        public static bool operator !=(Chunk left, Chunk right)
        {
            return !left.Equals(right);
        }

        #endregion
    }
}
