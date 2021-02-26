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
        //public Mesh mesh;
        //private MeshCube MeshCube;
        public VoxelEngine Engine;
        public bool MeshCreated { get; set; }
        public string ChunkName;

        public GameObject Entity;

        //Used to find voxel at position
        public float this[int x, int y, int z]
        {
            get => Voxels[PosToIndex(x, y, z)];
            set => Voxels[PosToIndex(x, y, z)] = value;
        }

        public void AddEntity(GameObject entity) => Entity = entity;

        public ChunkVoxelSetter CreateJob(Vector3 origin)
        {
            float resolution = Engine.ChunkInfo.VoxelSize;
            float scale = Engine.NoiseValues.SimplexOneScale;

            return new ChunkVoxelSetter
            {
                size = ChunkSize + 1,
                height = ChunkHeight + 1,
                heightMultiplier = 0,
                scale = scale,
                resolution = resolution,
                origin = origin,
                voxels = new NativeArray<float>((ChunkSize + 1) * (ChunkHeight + 1) * (ChunkSize + 1), Allocator.Persistent),
                seed = 0
            };
        }

        public void VoxelsFromJob(ChunkVoxelSetter job)
        {
            Voxels = job.voxels.ToArray();
            job.voxels.Dispose();
        }

        public void SetMesh( Vector3 origin)
        {
            var meshCreator = new MeshCreator(origin, Engine.ChunkInfo.VoxelSize);

            meshCreator.SetMesh(Voxels, origin.x, origin.y, origin.z,
                Engine.ChunkInfo.VoxelSize);

            var monoGo = Entity.GetComponent<MonoChunk>();
            
            var mesh = new Mesh();
            //Update mesh
            mesh.vertices = meshCreator.Vertices.ToArray();
            mesh.triangles = meshCreator.Triangles.ToArray();
            mesh.uv = new Vector2[mesh.vertices.Length];
            //mesh.SetColors(Colors);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            mesh.name = ChunkName;

            monoGo.MeshFilter.sharedMesh = mesh;
        }

        public static int PosToIndex(int x, int y, int z) => z * (ChunkSize + 1) * (ChunkHeight + 1) + y * (ChunkSize + 1) + x;

        public Chunk(float x, float y, float z, float size, VoxelEngine engine)
        {
            //mesh = new Mesh();
            Engine = engine;
            Voxels = new float[ChunkSize + 1 * ChunkHeight + 1 * ChunkSize + 1];
            //MeshCube = new MeshCube(this);
            //SetVoxel(x, y, z, size);
            
            CubeVertices = new Vector3[] {
                new Vector3 (0, 0, 0), //0
                new Vector3 (1 * Engine.ChunkInfo.VoxelSize, 0, 0), //1
                new Vector3 (1 * Engine.ChunkInfo.VoxelSize, 1 * Engine.ChunkInfo.VoxelSize, 0), //2
                new Vector3 (0, 1 * Engine.ChunkInfo.VoxelSize, 0), //3
                new Vector3 (0, 1 * Engine.ChunkInfo.VoxelSize, 1 * Engine.ChunkInfo.VoxelSize), //4
                new Vector3 (1 * Engine.ChunkInfo.VoxelSize, 1 * Engine.ChunkInfo.VoxelSize, 1 * Engine.ChunkInfo.VoxelSize), //5
                new Vector3 (1 * Engine.ChunkInfo.VoxelSize, 0, 1 * Engine.ChunkInfo.VoxelSize), //6
                new Vector3 (0, 0, 1 * Engine.ChunkInfo.VoxelSize), //7
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
            MeshCreated = false;
            ChunkName = "Chunk: " + x + ", " + y + ", " + z;
        }

        public Mesh AssignMesh()
        {
            var mesh = new Mesh();
            //Update mesh
            mesh.vertices = Vertices.ToArray();
            mesh.triangles = Triangles.ToArray();
            mesh.uv = new Vector2[mesh.vertices.Length];
            //mesh.SetColors(Colors);
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            
            mesh.name = ChunkName;
            MeshCreated = true;
            return mesh;
        }

        //Iterate through all voxels and set their type
        public void SetVoxel(float x, float y, float z, float size)
        {
            for(var i = 0; i < ChunkSize + 1; i++)
            {
                for(var k = 0; k < ChunkSize + 1; k++)
                {
                    for(var j = 0; j < ChunkHeight + 1; j++)
                    {
                        this[i, j, k] = SetVoxelType(x + (i * size), y + (j * size), z + (k * size));
                    }
                }
            }
            //MeshUpdate = true;
            CreateMesh(x, y, z);
        }
        
        //set individual voxel type using noise function
        public float SetVoxelType(float x, float y, float z)
        {
            var blockType = VoxelType.Default;
            if (Engine.UsePerlin)
            {
                var noise1 = PerlinNoise.Generate2DNoiseValue( x * 0.3f, z * 0.3f, Engine.NoiseValues.SimplexOneScale, 0, 0);
                var noise2 = PerlinNoise.Generate2DNoiseValue(x * 0.8f, z * 0.8f, Engine.NoiseValues.SimplexOneScale, 0, 0);
                var caveNoise1 = PerlinNoise.Generate3DNoiseValue(x * 0.8f, z * 0.8f, y * 0.8f, Engine.NoiseValues.SimplexOneScale, 0);
                var caveNoise2 = PerlinNoise.Generate3DNoiseValue(x * 0.3f, z * 0.3f, y * 0.3f, Engine.NoiseValues.SimplexOneScale, 0);
                var caveMask = PerlinNoise.Generate2DNoiseValue(x * 0.3f, z * 0.3f, Engine.NoiseValues.SimplexOneScale, 0, 0);

                var heightMap = noise1 + noise2;
                var caveHeight = caveNoise1 + caveNoise2;
                
                if (y <= heightMap)
                {
                    blockType = VoxelType.Dirt;

                    //just on the surface, use a grass type
                    if (y > heightMap - 1)
                    {
                        blockType = VoxelType.Grass;
                    }

                    //surface is above snow height, use snow type
                    if (y > Engine.VoxelTypeHeights.SnowHeight) blockType = VoxelType.Snow;
                    
                    //mask for generating caves

                    if (caveHeight > Mathf.Max(caveMask, .2f) && (y <= Engine.VoxelTypeHeights.CaveStartHeight ||
                                                               y < heightMap -
                                                               -Engine.VoxelTypeHeights.CaveStartHeight))
                        blockType = VoxelType.Default;
                    
                }
            }
            else
            {
                //3D noise for heightmap
                var simplex1 = Engine._fastNoise.GetNoise(x * 0.3f, z * 0.3f) * Engine.NoiseValues.SimplexOneScale;
                var simplex2 = Engine._fastNoise.GetNoise(x * 0.8f, z * 0.8f) * Engine.NoiseValues.SimplexTwoScale;

                //3d noise for caves and overhangs and such
                var caveNoise1 = Engine._fastNoise.GetNoise(x * 0.3f, y * 0.3f, z * 0.3f) *
                                 Engine.NoiseValues.CaveNoiseOneScale;
                var caveNoise2 = Engine._fastNoise.GetNoise(x * 0.8f, y * 0.8f, z * 0.8f) *
                                 Engine.NoiseValues.CaveNoiseTwoScale;
                var caveMask = Engine._fastNoise.GetNoise(x, z) + Engine.NoiseValues.CaveMask;

                //stone layer heightmap
                var simplexStone1 = Engine._fastNoise.GetNoise(x * 0.3f, z * 0.3f) *
                                    Engine.NoiseValues.SimplexStoneOneScale;
                var simplexStone2 = Engine._fastNoise.GetNoise(x * 0.8f, z * 0.8f) *
                                    Engine.NoiseValues.SimplexStoneTwoScale;

                var heightMap = simplex1 + simplex2;
                var caveMap = caveNoise1 + caveNoise2;
                var baseLandHeight = heightMap;
                var stoneHeightMap = simplexStone1 + simplexStone2;
                var baseStoneHeight = ChunkSize + stoneHeightMap;

                //under the surface, dirt block
                if (y <= baseLandHeight)
                {
                    //blockType = VoxelType.Dirt;
                    blockType = VoxelType.Dirt;

                    //just on the surface, use a grass type
                    if (y > baseLandHeight - 1)
                    {
                        blockType = VoxelType.Grass;
                    }

                    //surface is above snow height, use snow type
                    if (y > Engine.VoxelTypeHeights.SnowHeight) blockType = VoxelType.Snow;
                    
                    //too low for dirt, make it stone
                    if (y <= baseStoneHeight && y < baseLandHeight - Engine.VoxelTypeHeights.StoneDepth)
                        blockType = VoxelType.Stone;
                }

                //mask for generating caves

                if (caveMap > Mathf.Max(caveMask, .2f) && (y <= Engine.VoxelTypeHeights.CaveStartHeight ||
                                                           y < baseLandHeight -
                                                           -Engine.VoxelTypeHeights.CaveStartHeight))
                    blockType = VoxelType.Default;
                
            }

            return (float) blockType;
        }

        #region Mesh
        
        public readonly List<Vector3> Vertices;
        public readonly List<int> Triangles;
        public readonly List<Color> Colors;
        private static readonly Color32[] _colors = {new Color32(66, 177, 0, 255), new Color32(87, 51, 0, 255), new Color32(85, 85, 85, 255), new Color32(255, 176, 0, 255), new Color32(255, 255, 255, 255), new Color32(0, 0, 255, 255), new Color32(110, 70, 0, 255)  };
        private Vector3 _pos;
        private int _numFaces;
        private readonly Vector3[] CubeVertices;
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

        public async void CreateMesh(float x, float y, float z)
        {
            //Recalculate(16, 1, new Vector3(x, y, z), true);
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            
            await SetMeshData(x, y, z);
            
            //_chunk.CreateMesh();
        }
        
        public async void UpdateMesh(float x, float y, float z)
        {
            //Recalculate(16, 1, new Vector3(x, y, z), true);
            Vertices.Clear();
            Triangles.Clear();
            Colors.Clear();
            
            await UpdateMeshData(x, y, z);
            
            //_chunk.CreateMesh();
        }

        internal void Recalculate(int size, float scale, Vector3 origin, bool interpolate)
        {
            int flagIndex = 0;
            int index = 0;

            Vertices.Clear();
            Triangles.Clear();

            float[] afCubes = new float[8];

            for (int x = 0; x < ChunkSize - 1; x++)
            {
                for (int y = 0; y < ChunkHeight - 1; y++)
                {
                    for (int z = 0; z < ChunkSize - 1; z++)
                    {
                        //Offsets are same as cornerOffsets[8]
                        afCubes[0] = Voxels[PosToIndex(x, y, z)];
                        afCubes[1] = Voxels[PosToIndex(x + 1, y, z)];
                        afCubes[2] = Voxels[PosToIndex(x + 1, y + 1, z)];
                        afCubes[3] = Voxels[PosToIndex(x, y + 1, z)];
                        afCubes[4] = Voxels[PosToIndex(x, y, z + 1)];
                        afCubes[5] = Voxels[PosToIndex(x + 1, y, z + 1)];
                        afCubes[6] = Voxels[PosToIndex(x + 1, y + 1, z + 1)];
                        afCubes[7] = Voxels[PosToIndex(x, y + 1, z + 1)];


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
                                    float s1 = Voxels[PosToIndex(x + (int) edge1.x, y + (int) edge1.y, z + (int) edge1.z)];
                                    float delta = s1 - Voxels[PosToIndex(x + (int) edge2.x, y + (int) edge2.y, z + (int) edge2.z)];
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

        private async Task SetMeshData(float x, float y, float z)
        {
            for (var i = 0; i < ChunkSize; i++)
            {
                for (var j = 0; j < ChunkHeight; j++)
                {
                    for (var k = 0; k < ChunkSize; k++)
                    {
                        var voxelType = this[i, j, k];
                        // If it is air we ignore this block
                        if (voxelType == 0)
                            continue;
                        _pos = new Vector3(i, j, k) * Engine.ChunkInfo.VoxelSize;
                        // Remember current position in vertices list so we can add triangles relative to that
                        _numFaces = 0;

                        //for each face, check corresponding position for potential voxel type
                        //works for spaces where voxels don't currently exist
                        //neighbour checks will be required once destruction/construction is added to voxel mechanics

                        #region RightFace

                        if (SetVoxelType(x + ((i + 1) * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //right face
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


                        if (SetVoxelType(x + ((i - 1) * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //left face
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


                        if (SetVoxelType(x + (i * Engine.ChunkInfo.VoxelSize), y + ((j + 1) * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //top face
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

                        if (SetVoxelType(x + (i * Engine.ChunkInfo.VoxelSize), y + ((j - 1) * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //bottom face
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

                        if (SetVoxelType(x + (i * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + ((k + 1) * Engine.ChunkInfo.VoxelSize)) == 0) //back face
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

                        if (SetVoxelType(x + (i * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + ((k - 1) * Engine.ChunkInfo.VoxelSize)) == 0) //front face
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
        
        private async Task UpdateMeshData(float x, float y, float z)
        {
            for (var i = 0; i < ChunkSize; i++)
            {
                for (var j = 0; j < ChunkHeight; j++)
                {
                    for (var k = 0; k < ChunkSize; k++)
                    {
                        var voxelType = this[i, j, k];
                        // If it is air we ignore this block
                        if (voxelType == 0)
                            continue;
                        _pos = new Vector3(i, j, k) * Engine.ChunkInfo.VoxelSize;
                        // Remember current position in vertices list so we can add triangles relative to that
                        _numFaces = 0;

                        //for each face, check corresponding position for potential voxel type
                        //works for spaces where voxels don't currently exist
                        //neighbour checks will be required once destruction/construction is added to voxel mechanics

                        #region RightFace

                        if (GetNeighbourVoxel(x + ((i + 1) * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //right face
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


                        if (GetNeighbourVoxel(x + ((i - 1) * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //left face
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


                        if (GetNeighbourVoxel(x + (i * Engine.ChunkInfo.VoxelSize), y + ((j + 1) * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //top face
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

                        if (GetNeighbourVoxel(x + (i * Engine.ChunkInfo.VoxelSize), y + ((j - 1) * Engine.ChunkInfo.VoxelSize),
                            z + (k * Engine.ChunkInfo.VoxelSize)) == 0) //bottom face
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

                        if (GetNeighbourVoxel(x + (i * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + ((k + 1) * Engine.ChunkInfo.VoxelSize)) == 0) //back face
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

                        if (GetNeighbourVoxel(x + (i * Engine.ChunkInfo.VoxelSize), y + (j * Engine.ChunkInfo.VoxelSize),
                            z + ((k - 1) * Engine.ChunkInfo.VoxelSize)) == 0) //front face
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

        private float GetNeighbourVoxel(float x, float y, float z)
        {
            var voxelType = 0;
            
            var posX = Mathf.FloorToInt(x / Engine.ChunkSize) * Engine.ChunkSize;
            var posY = Mathf.FloorToInt(y / Engine.ChunkHeight) * Engine.ChunkHeight;
            var posZ = Mathf.FloorToInt(z / Engine.ChunkSize) * Engine.ChunkSize;

            var hasVoxel = Engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(posX, posY, posZ));

            if (hasVoxel)
            {
                var chunk = Engine.WorldData.Chunks[ChunkId.FromWorldPos(posX, posY, posZ)];
                var voxPosX = x - posX;
                var voxPosY = y - posY;
                var voxPosZ = z - posZ;
                voxelType = (int) chunk[(int) voxPosX, (int) voxPosY, (int) voxPosZ];
            }

            return voxelType;
        }
        
        #endregion
        
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
