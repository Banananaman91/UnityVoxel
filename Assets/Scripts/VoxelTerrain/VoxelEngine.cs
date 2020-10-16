using System.Collections;
using Noise;
using UnityEngine;

namespace VoxelTerrain
{
    public class VoxelEngine : MonoBehaviour
    {
        private World _world = new World();
        readonly int _chunkSize = 16;
        private float[][] _perlinNoiseArray;
        private FastNoiseLite _fastNoise = new FastNoiseLite();
        [SerializeField] private Material _material;
        [SerializeField] private int _chunkDistance = 10;
        [SerializeField] private int _chunkHeight = 10;
        [SerializeField] private Transform _origin;
        [SerializeField] private int _stoneDepth;
        [SerializeField] private int _snowHeight;
        [SerializeField] private int _caveStartHeight;

        private void Awake()
        {
            StartCoroutine(GenerateWorld());
        }

        private IEnumerator GenerateWorld()
        {
            var timeElapsed = 0f;
            for (var x = _origin.position.x - _chunkDistance; x <= _origin.position.x + _chunkDistance; x += _chunkSize)
            {
                for (var y = _origin.position.y - _chunkHeight; y <= _origin.position.y + _chunkHeight; y += _chunkSize)
                {
                    for (var z = _origin.position.z - _chunkDistance; z <= _origin.position.z + _chunkDistance; z += _chunkSize)
                    {
                        StartCoroutine(BuildChunk((int) x, (int) y, (int) z));
                        timeElapsed += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            Debug.Log("Time taken: " + timeElapsed);
        }

        private IEnumerator BuildChunk(int x, int y, int z)
        {
            var chunkGameObject = new GameObject("Chunk " + x + ", " + y + ", " + z);
            chunkGameObject.transform.position = new Vector3(x, y, z);
            chunkGameObject.transform.parent = transform.parent;
            var chunk = chunkGameObject.AddComponent<Chunk>();
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunkGameObject.GetComponent<MeshRenderer>().material = _material;
        
        
            for(int i = 0; i < _chunkSize; i++)
            {
                for(int k = 0; k < _chunkSize; k++)
                {
                    for(int j = 0; j < _chunkSize; j++)
                    {
                        chunk[i, j, k] = SetBlocks(x + i, j + y, k + z);
                    }
                }
            }
            var mesh = chunk.MeshCube.CreateMesh();
            chunk.MeshFilter.mesh = mesh;
            yield return null;
        }

        private BlockType SetBlocks(int x, int y, int z)
        {
            var simplex1 = _fastNoise.GetNoise(x*.8f, z*.8f)*10;
            var simplex2 = _fastNoise.GetNoise(x * 3f, z * 3f) * 10*(_fastNoise.GetNoise(x*.3f, z*.3f)+.5f);

            var heightMap = simplex1 + simplex2;
        
            //add the 2d noise to the middle of the terrain chunk
            var baseLandHeight = _chunkSize * 0.5f + heightMap;
            
            //3d noise for caves and overhangs and such
            var caveNoise1 = _fastNoise.GetNoise(x*5f, y*10f, z*5f);
            var caveMask = _fastNoise.GetNoise(x * .3f, z * .3f)+.3f;
            
            //stone layer heightmap
            var simplexStone1 = _fastNoise.GetNoise(x * 1f, z * 1f) * 10;
            var simplexStone2 = (_fastNoise.GetNoise(x * 5f, z * 5f)+.5f) * 20 * (_fastNoise.GetNoise(x * .3f, z * .3f) + .5f);

            var stoneHeightMap = simplexStone1 + simplexStone2;
            var baseStoneHeight = _chunkSize * 0.1f + stoneHeightMap;

            var blockType = BlockType.Default;

            //under the surface, dirt block
            if(y <= baseLandHeight)
            {
                blockType = BlockType.Dirt;

                //just on the surface, use a grass type
                if(y > baseLandHeight - 1) blockType = BlockType.Grass;

                if (y > _snowHeight) blockType = BlockType.Snow;

                if(y <= baseStoneHeight && y < baseLandHeight - _stoneDepth) blockType = BlockType.Stone;
            }
            if(caveNoise1 > Mathf.Max(caveMask, .2f) && y <= _caveStartHeight)
                blockType = BlockType.Default;

            return blockType;
        }
    }
}
