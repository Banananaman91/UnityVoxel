using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise;
using UnityEngine;
using VoxelTerrain.Dependencies;

namespace VoxelTerrain
{
    public class VoxelEngine : MonoBehaviour
    {
        public FastNoiseLite _fastNoise = new FastNoiseLite();
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField, Range(0.01f, 1.0f), Tooltip("Metre cubed")] private float _voxelSize = 1;
        [SerializeField] private Material _material;
        [SerializeField] private int _chunkDistance = 10;
        [SerializeField, Tooltip("Must be divisible by 32")] private int _chunkHeightDist = 32;
        [SerializeField] private Transform _origin;
        [SerializeField] public int _stoneDepth;
        [SerializeField] public int _snowHeight;
        [SerializeField] public int _caveStartHeight;
        private Vector3 _start = Vector3.zero;
        private Vector3 _curChunkPos;
        private List<ChunkId> _toDestroy = new List<ChunkId>();
        private List<Chunk> _chunkPool = new List<Chunk>();
        private Chunk _currentChunk;
    
        private bool _loaded;        
        private World _world = new World();
        private Vector3 Position => _origin.position;
        public float VoxelSize => _voxelSize;
        public float _chunkSize => 16 * _voxelSize;
        public float _chunkHeight => 32 * _voxelSize;

        

        private void Awake()
        {
            GenerateWorld();
        }

        private void Update()
        {
            if (!_loaded) return; // Don't run until world generation is complete

            //Get current position from origin
            var curChunkPosX = Mathf.FloorToInt(Position.x / _chunkSize) * _chunkSize;
            var curChunkPosY = Mathf.FloorToInt(Position.y / _chunkHeight) * _chunkHeight;
            var curChunkPosZ = Mathf.FloorToInt(Position.z / _chunkSize) * _chunkSize;

            //check if a chunk exists in current position
            var hasChunk =
                _world.Chunks.ContainsKey(ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ));
            if (!hasChunk) return;

            //If position has chunk, get chunky boi
            var chunk = _world.Chunks[ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ)];

            //Set current chunk if one hasn't already been set
            if (!_currentChunk) _currentChunk = chunk;

            //Chunk comparison, update terrain if current chunk doesn't match
            if (chunk == _currentChunk) return;
            _currentChunk = chunk;
            _curChunkPos = _currentChunk.transform.position;
            ExpandTerrain();
        }

        private void ExpandTerrain()
        {
            _toDestroy.Clear();

            //Collect all chunks that are out of range to be 'destroyed'
            foreach (var chunk in _world.Chunks)
            {
                if (Mathf.Abs(_curChunkPos.x - chunk.Key.X) > _chunkDistance ||
                    Mathf.Abs(_curChunkPos.y - chunk.Key.Y) > _chunkHeightDist ||
                    Mathf.Abs(_curChunkPos.z - chunk.Key.Z) > _chunkDistance)
                {
                    _toDestroy.Add(chunk.Key);
                }
            }

            //Iterate through _toDestroy and return chunks to the chunk pool to be reused
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

            //Iterate through all positions within range, if that position doesn't have a chunk then set one there
            for (var x = _curChunkPos.x - _chunkDistance; x <= _curChunkPos.x + _chunkDistance; x += _chunkSize)
            {
                for (var y = _curChunkPos.y - _chunkHeightDist; y <= _curChunkPos.y + _chunkHeightDist; y += _chunkHeight)
                {
                    for (var z = _curChunkPos.z - _chunkDistance; z <= _curChunkPos.z + _chunkDistance; z += _chunkSize)
                    {
                        if (_world.Chunks.ContainsKey(ChunkId.FromWorldPos(x, y, z))) continue;
                        BuildChunk( x, y, z);
                    }
                }
            }

            //yield return null;
        }

        //Build the world, the whole world, in his hands
        private void GenerateWorld()
        {
            var timeElapsed = 0f;
            for (var x = _start.x - _chunkDistance; x <= _start.x + _chunkDistance; x += _chunkSize)
            {
                //splits y and z for each row into a separate method, seemed to be faster
                GenerateRow(x);
                timeElapsed += Time.deltaTime;
            }
            //Uncomment debug log if you wish to see how fast it is (spoiler alert, Sonic speed)
            //Debug.Log("Time taken: " + timeElapsed);

            //World generation complete
            _loaded = true;
        }

        //Method for each x row, generate all of the y and z parts
        private void GenerateRow(float x)
        {
            for (var y = _start.y - _chunkHeightDist; y <= _start.y + _chunkHeightDist; y += _chunkHeight)
            {
                for (var z = _start.z - _chunkDistance; z <= _start.z + _chunkDistance; z += _chunkSize)
                {
                    CreateNewChunkObject(x, y, z);
                }
            }
        }

        //Instantiate a new chunk
        private Chunk CreateNewChunkObject(float x, float y, float z)
        {
            //Uncomment next line if you wish to not use prefabs
            //var chunkGameObject = new GameObject("Chunk " + x + ", " + y + ", " + z);

            //Comment out next lines if you wish to not use prefabs
            var chunkGameObject = Instantiate(_chunkPrefab, new Vector3(x, y, z), Quaternion.identity);
            chunkGameObject.name = "Chunk: " + x + ", " + y + ", " + z;

            var transform1 = chunkGameObject.transform;

            //Uncomment next line if you wish to not use prefabs
            //transform1.position = new Vector3(x, y, z);

            //Set parent, tidy heirarchy, everyone is happy
            transform1.parent = transform;

            //Uncomment next line if you wish to not use prefabs
            //var chunk = chunkGameObject.AddComponent<Chunk>();

            //Comment out next line if you wish to not use prefabs
            var chunk = chunkGameObject.GetComponent<Chunk>();

            chunk.IsAvailable = false;
            _chunkPool.Add(chunk);
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunkGameObject.GetComponent<MeshRenderer>().material = _material;
            
            var t = new Task(() => chunk.SetVoxel(x, y, z, _voxelSize));
            t.Start();

            return chunk;
        }

        private void ReturnChunkToPool(Chunk target) => target.IsAvailable = true;

        private Chunk GetChunkObject(float X, float Y, float Z)
        {
            var chunk = _chunkPool.FirstOrDefault(x => x.IsAvailable);
            if (chunk == null)
            {
                //contingency, creates new chunk if pool has none available. Logically shouldn't run, but you never know
                chunk = CreateNewChunkObject(X, Y, Z);
            }

            chunk.IsAvailable = false;
            return chunk;
        }

        //Grab available chunk from the pool, update its information and position
        private void BuildChunk(float x, float y, float z)
        {
            var chunk = GetChunkObject(x, y, z);
            chunk.name = "Chunk " + x + ", " + y + ", " + z;
            var transform1 = chunk.transform;
            transform1.position = new Vector3(x, y, z);
            transform1.parent = transform;
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunk.MeshRender.material = _material;
            
            var t = new Task(() => chunk.SetVoxel(x, y, z, _voxelSize));
            t.Start();
        }
    }
}
