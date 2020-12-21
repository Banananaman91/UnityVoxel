using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise;
using UnityEngine;

namespace VoxelTerrain
{
    public class VoxelEngine : MonoBehaviour
    {
        private World _world = new World();
        public float VoxelSize => _voxelSize;
        public float _chunkSize => 16 * _voxelSize;
        public float _chunkHeight => 32 * _voxelSize;
        public FastNoiseLite _fastNoise = new FastNoiseLite();
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField, Range(0.01f, 1.0f)] private float _voxelSize = 1;
        [SerializeField] private Material _material;
        [SerializeField] private int _chunkDistance = 10;
        [SerializeField, Tooltip("Must be divisible by height")] private int _chunkHeightDist = 32;
        
        [SerializeField] private Transform _origin;
        [SerializeField] public int _stoneDepth;
        [SerializeField] public int _snowHeight;
        [SerializeField] public int _caveStartHeight;

        private Vector3 _start = Vector3.zero;
        
        private List<ChunkId> _toDestroy = new List<ChunkId>();
        public List<Chunk> _chunkPool = new List<Chunk>();
        public Chunk _currentChunk;
        private Vector3 _curChunkPos;

        private Vector3 Position => _origin.position;

        public bool _loaded;        

        private void Awake()
        {
            Debug.Log(_chunkSize);
            GenerateWorld();
        }

        private void Update()
        {
            if (!_loaded) return;

            var curChunkPosX = Mathf.FloorToInt(Position.x / _chunkSize) * _chunkSize;
            var curChunkPosY = Mathf.FloorToInt(Position.y / _chunkHeight) * _chunkHeight;
            var curChunkPosZ = Mathf.FloorToInt(Position.z / _chunkSize) * _chunkSize;

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
                        if (_world.Chunks.ContainsKey(ChunkId.FromWorldPos(x, y, z))) continue;
                        BuildChunk( x, y, z);
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
                GenerateRow(x);
                timeElapsed += Time.deltaTime;
            }
            Debug.Log("Time taken: " + timeElapsed);
            _loaded = true;
        }

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

        private Chunk CreateNewChunkObject(float x, float y, float z)
        {
            //var chunkGameObject = new GameObject("Chunk " + x + ", " + y + ", " + z);
            var chunkGameObject = Instantiate(_chunkPrefab, new Vector3(x, y, z), Quaternion.identity);
            chunkGameObject.name = "Chunk: " + x + ", " + y + ", " + z;
            var transform1 = chunkGameObject.transform;
            //transform1.position = new Vector3(x, y, z);
            transform1.parent = transform;
            //var chunk = chunkGameObject.AddComponent<Chunk>();
            var chunk = chunkGameObject.GetComponent<Chunk>();
            chunk.IsAvailable = false;
            _chunkPool.Add(chunk);
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunkGameObject.GetComponent<MeshRenderer>().material = _material;
            
            var t = new Task(() => chunk.SetBlock(x, y, z, _voxelSize));
            t.Start();

            return chunk;
        }

        private void ReturnChunkToPool(Chunk target) => target.IsAvailable = true;

        private Chunk GetChunkObject(float X, float Y, float Z)
        {
            var chunk = _chunkPool.FirstOrDefault(x => x.IsAvailable);
            if (chunk == null)
            {
                chunk = CreateNewChunkObject(X, Y, Z);
            }

            chunk.IsAvailable = false;
            return chunk;
        }

        private void BuildChunk(float x, float y, float z)
        {
            var chunk = GetChunkObject(x, y, z);
            chunk.name = "Chunk " + x + ", " + y + ", " + z;
            var transform1 = chunk.transform;
            transform1.position = new Vector3(x, y, z);
            transform1.parent = transform;
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunk.MeshRender.material = _material;
            
            var t = new Task(() => chunk.SetBlock(x, y, z, _voxelSize));
            t.Start();
        }
    }
}
