using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise;
using UnityEngine;
using UnityEngine.AI;

namespace VoxelTerrain
{
    public class VoxelEngine : MonoBehaviour
    {
        
        private World _world = new World();
        readonly int _chunkSize = 16;
        private float[][] _perlinNoiseArray;
        public FastNoiseLite _fastNoise = new FastNoiseLite();
        [SerializeField] private Material _material;
        [SerializeField] private int _chunkDistance = 10;
        [SerializeField, Tooltip("Must be divisible by 32")] private int _chunkHeightDist = 32;
        [SerializeField] private int _chunkHeight = 32;
        [SerializeField] private Transform _origin;
        [SerializeField] public int _stoneDepth;
        [SerializeField] public int _snowHeight;
        [SerializeField] public int _caveStartHeight;

        private Vector3 _start = Vector3.zero;
        
        private List<ChunkId> _toDestroy = new List<ChunkId>();
        private List<Chunk> _chunkPool = new List<Chunk>();
        public Chunk _currentChunk;
        private Vector3 _curChunkPos;

        private Vector3 Position => _origin.position;

        private bool _loaded;

        private Vector3 _cp;

        //NAV SHIT 
        public GameObject agent;

        #region NoiseVariables

        public float simplex1;
        public float simplex2;

        public float heightMap => simplex1 + simplex2;
        
        //add the 2d noise to the middle of the terrain chunk
        public float baseLandHeight => _chunkSize * 0.5f + heightMap;
            
        //3d noise for caves and overhangs and such
        public float caveNoise1;
        public float caveMask;
            
        //stone layer heightmap
        public float simplexStone1;
        public float simplexStone2;

        public float stoneHeightMap => simplexStone1 + simplexStone2;
        public float baseStoneHeight => _chunkSize * 0.1f + stoneHeightMap;

        public BlockType blockType = BlockType.Default;

        #endregion

        private void Awake()
        {
            
            GenerateWorld();
           
        }
        private void Start()
        {

        }
        private void Update()
        {
            if (!_loaded) return;

            var curChunkPosX = Mathf.FloorToInt(Position.x / 16) * 16;
            var curChunkPosY = Mathf.FloorToInt(Position.y / 32) * 32;
            var curChunkPosZ = Mathf.FloorToInt(Position.z / 16) * 16;

            var hasChunk =
                _world.Chunks.ContainsKey(ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ));
            if (!hasChunk) return;
            var chunk = _world.Chunks[ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ)];
            if (!_currentChunk) _currentChunk = chunk;

            if (chunk == _currentChunk) return;
            _currentChunk = chunk;
            _curChunkPos = _currentChunk.transform.position;
            ExpandTerrain();
        }

        private void ExpandTerrain()
        {
            _toDestroy.Clear();

            foreach (var chunk in _world.Chunks)
            {
                if (Mathf.Abs(_curChunkPos.x - chunk.Key.X) > _chunkDistance ||
                    Mathf.Abs(_curChunkPos.y - chunk.Key.Y) > _chunkHeightDist ||
                    Mathf.Abs(_curChunkPos.z - chunk.Key.Z) > _chunkDistance)
                {
                    _toDestroy.Add(chunk.Key);
                }
            }

            if (_toDestroy.Count != 0)
            {
                foreach (var chunk in _toDestroy)
                {
                    if (!_world.Chunks.ContainsKey(chunk)) continue;
                    var dChunk = _world.Chunks[chunk];
                    _world.Chunks.Remove(chunk);
                    ReturnChunkToPool(dChunk);
                }
            }

            _curChunkPos = _currentChunk.transform.position;

            for (var x = _curChunkPos.x - _chunkDistance; x <= _curChunkPos.x + _chunkDistance; x += _chunkSize)
            {
                for (var y = _curChunkPos.y - _chunkHeightDist; y <= _curChunkPos.y + _chunkHeightDist; y += _chunkHeight)
                {
                    for (var z = _curChunkPos.z - _chunkDistance; z <= _curChunkPos.z + _chunkDistance; z += _chunkSize)
                    {
                        if (_world.Chunks.ContainsKey(ChunkId.FromWorldPos((int) x, (int) y, (int) z))) continue;
                        BuildChunk((int) x, (int) y, (int) z);
                    }
                }
            }

            //yield return null;
        }

        private void GenerateWorld()
        {
            var timeElapsed = 0f;
            for (var x = _start.x - _chunkDistance; x <= _start.x + _chunkDistance; x += _chunkSize)
            {
                GenerateRow((int) x);
                timeElapsed += Time.deltaTime;
            }
            Debug.Log("Time taken: " + timeElapsed);
            _loaded = true;
        }

        private void GenerateRow(int x)
        {
            for (var y = _start.y - _chunkHeightDist; y <= _start.y + _chunkHeightDist; y += _chunkHeight)
            {
                for (var z = _start.z - _chunkDistance; z <= _start.z + _chunkDistance; z += _chunkSize)
                {
                    CreateNewChunkObject(x, (int) y, (int) z);
                    
                }
            }
            
        }

        private Chunk CreateNewChunkObject(int x, int y, int z)
        {
            var chunkGameObject = new GameObject("Chunk " + x + ", " + y + ", " + z);
            chunkGameObject.transform.position = new Vector3(x, y, z);
            chunkGameObject.transform.parent = transform.parent;
            var chunk = chunkGameObject.AddComponent<Chunk>();
            chunk.Engine = this;
            chunk.IsAvailable = false;
            _chunkPool.Add(chunk);
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunkGameObject.GetComponent<MeshRenderer>().material = _material;
            chunkGameObject.AddComponent<NavMeshSurface>();
            
            var t = new Task(() => chunk.SetBlock(x, y, z));
            t.Start();
            
            // for(var i = 0; i < _chunkSize; i++)
            // {
            //     for(var k = 0; k < _chunkSize; k++)
            //     {
            //         for(var j = 0; j < _chunkHeight; j++)
            //         {
            //             chunk[i, j, k] = SetBlocks(x + i, j + y, k + z);
            //         }
            //     }
            // }
            // chunk.MeshCube.CreateMesh();
            return chunk;
        }

        private void ReturnChunkToPool(Chunk target) => target.IsAvailable = true;

        private Chunk GetChunkObject(int X, int Y, int Z)
        {
            var chunk = _chunkPool.FirstOrDefault(x => x.IsAvailable);
            if (chunk == null)
            {
                chunk = CreateNewChunkObject(X, Y, Z);
            }

            chunk.IsAvailable = false;
            return chunk;
        }

        private void BuildChunk(int x, int y, int z)
        {
            var chunk = GetChunkObject(x, y, z);
            chunk.name = "Chunk " + x + ", " + y + ", " + z;
            var transform1 = chunk.transform;
            transform1.position = new Vector3(x, y, z);
            transform1.parent = transform;
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunk.MeshRender.material = _material;
            
            var t = new Task(() => chunk.SetBlock(x, y, z));
            t.Start();
            
            // for(var i = 0; i < _chunkSize; i++)
            // {
            //     for(var k = 0; k < _chunkSize; k++)
            //     {
            //         for(var j = 0; j < _chunkHeight; j++)
            //         {
            //             chunk[i, j, k] = SetBlocks(x + i, j + y, k + z);
            //         }
            //     }
            // }
            // chunk.MeshCube.CreateMesh();
        }
       
        private BlockType SetBlocks(int x, int y, int z)
        {
            simplex1 = _fastNoise.GetNoise(x*.8f, z*.8f)*10;
            simplex2 = _fastNoise.GetNoise(x * 3f, z * 3f) * 10*(_fastNoise.GetNoise(x*.3f, z*.3f)+.5f);

            //3d noise for caves and overhangs and such
            caveNoise1 = _fastNoise.GetNoise(x*5f, y*10f, z*5f);
            caveMask = _fastNoise.GetNoise(x * .3f, z * .3f)+.3f;
            
            //stone layer heightmap
            simplexStone1 = _fastNoise.GetNoise(x * 1f, z * 1f) * 10;
            simplexStone2 = (_fastNoise.GetNoise(x * 5f, z * 5f)+.5f) * 20 * (_fastNoise.GetNoise(x * .3f, z * .3f) + .5f);

            blockType = BlockType.Default;

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
