using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

        private Vector3 _start = Vector3.zero;
        
        private List<Vector3> _chunkPool = new List<Vector3>();
        public Chunk currentChunk;

        private Vector3 position => _origin.position;

        private bool _loaded;

        private void Awake()
        {
            StartCoroutine(GenerateWorld());
        }

        private void Update()
        {
            if (!_loaded) return;

            var curChunkPosX = Mathf.FloorToInt(position.x / 16) * 16;
            var curChunkPosY = Mathf.FloorToInt(position.y / 16) * 16;
            var curChunkPosZ = Mathf.FloorToInt(position.z / 16) * 16;

            var hasChunk =
                _world.Chunks.ContainsKey(ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ));
            Debug.Log(ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ));
            if (!hasChunk) return;
            var chunk = _world.Chunks[ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ)];
            if (!currentChunk) currentChunk = chunk;

            if (chunk == currentChunk) return;
            currentChunk = chunk;
            StartCoroutine(ExpandTerrain());
        }

        private IEnumerator ExpandTerrain()
        {
            var chunkTransPos = currentChunk.transform.position;

            for (var x = chunkTransPos.x - _chunkDistance; x <= chunkTransPos.x + _chunkDistance; x += _chunkSize)
            {
                for (var y = chunkTransPos.y - _chunkHeight; y <= chunkTransPos.y + _chunkHeight; y += _chunkSize)
                {
                    for (var z = chunkTransPos.z - _chunkDistance; z <= chunkTransPos.z + _chunkDistance; z += _chunkSize)
                    {
                        var hasChunk = _world.Chunks.ContainsKey(ChunkId.FromWorldPos((int) x, (int) y, (int) z));
                        if (hasChunk) continue;
                        _chunkPool.Add(new Vector3(x, y, z));
                        yield return null;
                    }
                }
            }

            var toDestroy = new List<ChunkId>();

            foreach (var chunk in _world.Chunks)
            {
                var cp = chunk.Value.transform.position;
                if (Mathf.Abs(chunkTransPos.x - cp.x) > _chunkDistance ||
                    Mathf.Abs(chunkTransPos.y - cp.y) > _chunkDistance ||
                    Mathf.Abs(chunkTransPos.z - cp.z) > _chunkDistance)
                {
                    toDestroy.Add(chunk.Key);
                }
            }

            foreach (var chunk in toDestroy)
            {
                var dChunk = _world.Chunks[chunk];
                _world.Chunks.Remove(chunk);
                Destroy(dChunk.gameObject);
            }

            if (_chunkPool.Count == 0) yield break;
            for (int i = _chunkPool.Count - 1; i > 0; i--)
            {
                var chunkPos = _chunkPool[i];
                if (!_world.Chunks.ContainsKey(ChunkId.FromWorldPos((int) chunkPos.x, (int) chunkPos.y,
                    (int) chunkPos.z)))
                {
                    StartCoroutine(BuildChunk((int) chunkPos.x, (int) chunkPos.y, (int) chunkPos.z));
                }

                _chunkPool.Remove(chunkPos);
            }
        }
        
        private IEnumerator GenerateWorld()
        {
            var timeElapsed = 0f;
            for (var x = _start.x - _chunkDistance; x <= _start.x + _chunkDistance; x += _chunkSize)
            {
                for (var y = _start.y - _chunkHeight; y <= _start.y + _chunkHeight; y += _chunkSize)
                {
                    for (var z = _start.z - _chunkDistance; z <= _start.z + _chunkDistance; z += _chunkSize)
                    {
                        StartCoroutine(BuildChunk((int) x, (int) y, (int) z));
                        timeElapsed += Time.deltaTime;
                        yield return null;
                    }
                }
            }
            Debug.Log("Time taken: " + timeElapsed);
            _loaded = true;
        }

        private IEnumerator BuildChunk(int x, int y, int z)
        {
            var chunkGameObject = new GameObject("Chunk " + x + ", " + y + ", " + z);
            chunkGameObject.transform.position = new Vector3(x, y, z);
            chunkGameObject.transform.parent = transform.parent;
            var chunk = chunkGameObject.AddComponent<Chunk>();
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunkGameObject.GetComponent<MeshRenderer>().material = _material;
            
            for(var i = 0; i < _chunkSize; i++)
            {
                for(var k = 0; k < _chunkSize; k++)
                {
                    for(var j = 0; j < _chunkSize; j++)
                    {
                        chunk[i, j, k] = SetBlocks(x + i, j + y, k + z);
                    }
                }
            }
            chunk.MeshFilter.mesh = chunk.MeshCube.CreateMesh();
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
