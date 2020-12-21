using System;
using MMesh;
using UnityEngine;

namespace VoxelTerrain
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class Chunk : MonoBehaviour
    {
        public const int ChunkSize = 16;
        public const int ChunkHeight = 32;
        private BlockType[,,] Voxels = new BlockType[ChunkSize,ChunkHeight,ChunkSize];
    
        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        public MeshRenderer MeshRender => GetComponent<MeshRenderer>();

        public VoxelEngine Engine;
        public bool MeshUpdate;
        private Mesh mesh;

        public bool IsAvailable { get; set; }

        public BlockType this[int x, int y, int z]
        {
            get => Voxels[x, y, z];
            set => Voxels[x, y, z] = value;
        }

        public MeshCube MeshCube;

        public void Awake()
        {
            Engine = FindObjectOfType<VoxelEngine>();
            MeshCube = new MeshCube(this);
        }

        public void LateUpdate()
        {
            if (!Engine || !MeshUpdate) return;
            //MeshCube.CreateMesh();
            mesh = new Mesh();
            mesh.SetVertices(MeshCube.Vertices);
            mesh.SetTriangles(MeshCube.Triangles.ToArray(), 0);
            mesh.SetColors(MeshCube.Colors);
            mesh.RecalculateNormals();
            MeshFilter.mesh = mesh;
            MeshUpdate = false;
        }

        public void SetBlock(float x, float y, float z, float size)
        {
            for(var i = 0; i < ChunkSize; i++)
            {
                for(var k = 0; k < ChunkSize; k++)
                {
                    for(var j = 0; j < ChunkHeight; j++)
                    {
                        this[i, j, k] = SetBlocks(x + (i * size), y + (j * size), z + (k * size));
                    }
                }
            }

            //MeshUpdate = true;
            MeshCube.CreateMesh();
        }
        
        private BlockType SetBlocks(float x, float y, float z)
        {
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

                if (y > Engine._snowHeight) blockType = BlockType.Snow;

                if(y <= baseStoneHeight && y < baseLandHeight - Engine._stoneDepth) blockType = BlockType.Stone;
            }
            if(caveNoise1 > Mathf.Max(caveMask, .2f) && y <= Engine._caveStartHeight)
               blockType = BlockType.Default;

            return blockType;
        }
    }
}
