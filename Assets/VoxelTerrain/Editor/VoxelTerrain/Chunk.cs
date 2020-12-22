using System;
using MMesh;
using UnityEngine;

namespace VoxelTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        public const int ChunkSize = 16; //Leave at this size
        public const int ChunkHeight = 32; //This should be 16 too, but I wanted taller chunks
        [HideInInspector] public VoxelEngine Engine;
        private BlockType[,,] Voxels = new BlockType[ChunkSize,ChunkHeight,ChunkSize];
        private Mesh mesh;
        private MeshCube MeshCube;
        public bool IsAvailable { get; set; }
        public bool MeshUpdate { get; set; }
        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        public MeshRenderer MeshRender => GetComponent<MeshRenderer>();

        //Used to find voxel at position
        public BlockType this[int x, int y, int z]
        {
            get => Voxels[x, y, z];
            set => Voxels[x, y, z] = value;
        }

        public void Awake()
        {
            //Find engine and create mesh cube
            Engine = FindObjectOfType<VoxelEngine>();
            MeshCube = new MeshCube(this);
        }

        public void LateUpdate()
        {
            if (!Engine || !MeshUpdate) return; //stops running if Engine is not found or MeshUpdate isn't required

            //Update mesh
            mesh = new Mesh();
            mesh.SetVertices(MeshCube.Vertices);
            mesh.SetTriangles(MeshCube.Triangles.ToArray(), 0);
            mesh.SetColors(MeshCube.Colors);
            mesh.RecalculateNormals();
            MeshFilter.mesh = mesh;
            MeshUpdate = false;
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
            MeshCube.CreateMesh(x, y, z);
        }
        
        //set individual voxel type using noise function
        public BlockType SetVoxelType(float x, float y, float z)
        {
            //3D noise for heightmap
            var simplex1 = Engine._fastNoise.GetNoise(x*.8f, z*.8f)*10;
            var simplex2 = Engine._fastNoise.GetNoise(x * 3f, z * 3f) * 10*(Engine._fastNoise.GetNoise(x*.3f, z*.3f)+.5f);

            //3d noise for caves and overhangs and such
            var caveNoise1 = Engine._fastNoise.GetNoise(x*5f, y*10f, z*5f);
            var caveMask = Engine._fastNoise.GetNoise(x * .3f, z * .3f)+.3f;
            
            //stone layer heightmap
            var simplexStone1 = Engine._fastNoise.GetNoise(x * 1f, z * 1f) * 10;
            var simplexStone2 = (Engine._fastNoise.GetNoise(x * 5f, z * 5f)+.5f) * 20 * (Engine._fastNoise.GetNoise(x * .3f, z * .3f) + .5f);
            
            var heightMap = simplex1 + simplex2;
            var baseLandHeight = ChunkSize * 0.5f + heightMap;
            var stoneHeightMap = simplexStone1 + simplexStone2;
            var baseStoneHeight = ChunkSize * 0.1f + stoneHeightMap;

            var blockType = BlockType.Default;

            //under the surface, dirt block
            if(y <= baseLandHeight)
            {
                blockType = BlockType.Dirt;

                //just on the surface, use a grass type
                if(y > baseLandHeight - 1) blockType = BlockType.Grass;

                //surface is above snow height, use snow type
                if (y > Engine._snowHeight) blockType = BlockType.Snow;

                //too low for dirt, make it stone
                if(y <= baseStoneHeight && y < baseLandHeight - Engine._stoneDepth) blockType = BlockType.Stone;
            }

            //mask for generating caves
            if(caveNoise1 > Mathf.Max(caveMask, .2f) && y <= Engine._caveStartHeight)
               blockType = BlockType.Default;

            return blockType;
        }
    }
}
