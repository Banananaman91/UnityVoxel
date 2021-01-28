using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Noise;
using UnityEngine;
using UnityEngine.UI;
using VoxelTerrain.Dependencies;
using VoxelTerrain.Editor.Voxel;

namespace VoxelTerrain
{
    public class VoxelEngine : MonoBehaviour
    {
        public FastNoiseLite _fastNoise = new FastNoiseLite();

        [Header("World Info")]
        [SerializeField] private Material _material;
        [SerializeField] private float _distance = 10;
        [SerializeField, Tooltip("Must be divisible by 32")] private float _height = 32;
        [SerializeField] private Transform _origin;

        [Header("Chunk Info")]
        [SerializeField] private MonoChunk _chunkPrefab;
        [SerializeField, Range(0.01f, 1.0f), Tooltip("Metre cubed")] private float _voxelSize = 1;

        [Header("Noise Info")]
        [SerializeField] private FastNoiseLite.NoiseType _noiseType;
        [SerializeField] private bool _setCustomSeed;
        [SerializeField] private int _seedValue;
        [SerializeField] private bool _randomSeed;
        [SerializeField] private float _noiseFrequency = 0.01f;
        [SerializeField] private FastNoiseLite.RotationType3D _rotationType3D = FastNoiseLite.RotationType3D.None;
        [SerializeField] private FastNoiseLite.TransformType3D _transformType3D = FastNoiseLite.TransformType3D.DefaultOpenSimplex2;

        [Header("Fractal")]
        [SerializeField] private FastNoiseLite.FractalType _fractalType = FastNoiseLite.FractalType.None;
        [SerializeField] private int _octaves = 3;
        [SerializeField] private float _lacunarity = 2.0f;
        [SerializeField] private float _gain = 0.5f;
        [SerializeField] private float _weightedStrength = 0.0f;
        [SerializeField] private float _pingPongStength = 2.0f;

        [Header("Cellular")]
        [SerializeField] private FastNoiseLite.CellularDistanceFunction _cellularDistanceFunction = FastNoiseLite.CellularDistanceFunction.EuclideanSq;
        [SerializeField] private FastNoiseLite.CellularReturnType _cellularReturnType = FastNoiseLite.CellularReturnType.Distance;
        [SerializeField] private float _cellularJitterModifier = 1.0f;

        [Header("Domain Warp")]
        [SerializeField] private FastNoiseLite.DomainWarpType _domainWarpType = FastNoiseLite.DomainWarpType.OpenSimplex2;
        [SerializeField] private FastNoiseLite.TransformType3D _warpTransformType3D = FastNoiseLite.TransformType3D.DefaultOpenSimplex2;
        [SerializeField] private float _domainWarpAmp = 1.0f;

        [Header("Voxel Type Heights")]
        [SerializeField] private float _stoneDepth;
        [SerializeField] private float _snowHeight;
        [SerializeField] private float _caveStartHeight;

        [Header("Noise Values")]
        [SerializeField] private float _simplexOneScale = 10;
        [SerializeField] private float _simplexTwoScale = 10;
        [SerializeField] private float _caveNoiseOneScale = 10;
        [SerializeField] private float _caveNoiseTwoScale = 10;
        [SerializeField] private float _caveMask = 0.3f;
        [SerializeField] private float _simplexStoneOneScale = 10;
        [SerializeField] private float _simplexStoneTwoScale = 20;

        [Header("Progress Bar")] 
        [SerializeField] private Slider _progress;
        private Vector3 _start;
        private Vector3 _curChunkPos;
        private List<MonoChunk> _toDestroy = new List<MonoChunk>();
        public List<MonoChunk> _chunkPool = new List<MonoChunk>();
        private Chunk _currentChunk { get; set; }
    
        private bool _chunksloaded;
        private bool _worldLoaded;
        private World _world = new World();

        private Vector3 Position => _origin != null ? _origin.position : Vector3.zero;
        public float VoxelSize => _voxelSize;
        public float _chunkSize => 16 * _voxelSize;
        public float _chunkHeight => 32 * _voxelSize;

        public float StoneDepth => _stoneDepth;
        public float SnowHeight => _snowHeight;
        public float CaveStartHeight => _caveStartHeight;

        public float SimplexOneScale => _simplexOneScale;
        public float SimplexTwoScale => _simplexTwoScale;
        public float CaveNoiseOneScale => _caveNoiseOneScale;
        public float CaveNoiseTwoScale => _caveNoiseTwoScale;
        public float CaveMask => _caveMask;
        public float SimplexStoneOneScale => _simplexStoneOneScale;
        public float SimplexStoneTwoScale => _simplexStoneTwoScale;

        private bool _randomSeedActive;
        private bool _customSeedActive;

        private void OnValidate() 
        {
            if (_randomSeed || _setCustomSeed)
            {
                if (_randomSeedActive && _setCustomSeed)
                {
                    _randomSeed = false;
                    _randomSeedActive = false;
                    _customSeedActive = true;
                }
                else if (_customSeedActive && _randomSeed)
                {
                    _setCustomSeed = false;
                    _customSeedActive = false;
                    _randomSeedActive = true;
                }
                else if (!_randomSeedActive && _randomSeed)
                {
                    _randomSeedActive = true;
                }
                else if (!_customSeedActive && _setCustomSeed)
                {
                    _customSeedActive = true;
                }
            }
        }

        private void Awake()
        {
            _start = transform.position;
            if (_randomSeed) _fastNoise.SetSeed(UnityEngine.Random.Range(0, 9999));
            else if (_setCustomSeed) _fastNoise.SetSeed(_seedValue);
            _fastNoise.SetNoiseType(_noiseType);
            _fastNoise.SetFrequency(_noiseFrequency);
            _fastNoise.SetRotationType3D(_rotationType3D);
            _fastNoise.SetFractalType(_fractalType);
            _fastNoise.SetFractalOctaves(_octaves);
            _fastNoise.SetFractalLacunarity(_lacunarity);
            _fastNoise.SetFractalGain(_gain);
            _fastNoise.SetFractalWeightedStrength(_weightedStrength);
            _fastNoise.SetFractalPingPongStrength(_pingPongStength);
            _fastNoise.SetCellularDistanceFunction(_cellularDistanceFunction);
            _fastNoise.SetCellularReturnType(_cellularReturnType);
            _fastNoise.SetCellularJitter(_cellularJitterModifier);
            _fastNoise.SetDomainWarpType(_domainWarpType);
            _fastNoise.SetDomainWarpAmp(_domainWarpAmp);
            StartCoroutine(GenerateWorldData()); 
        }

        private void Update()
        {
            if (!_chunksloaded) return; // Don't run until world generation is complete

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
            if (_currentChunk == null) _currentChunk = chunk;
            
            //Chunk comparison, update terrain if current chunk doesn't match
            if (chunk == _currentChunk) return;
            _currentChunk = chunk;
            _curChunkPos = new Vector3(curChunkPosX, curChunkPosY, curChunkPosZ);
            ExpandTerrain();
        }

        private void ExpandTerrain()
        {
            _toDestroy.Clear();

            //Collect all chunks that are out of range to be 'destroyed'
            foreach (var chunk in _chunkPool)
            {
                if (Mathf.Abs(_curChunkPos.x - chunk.transform.position.x) > _distance ||
                    Mathf.Abs(_curChunkPos.y - chunk.transform.position.y) > _height ||
                    Mathf.Abs(_curChunkPos.z - chunk.transform.position.z) > _distance)
                {
                    _toDestroy.Add(chunk);
                }
            }

            //Iterate through _toDestroy and return chunks to the chunk pool to be reused
            if (_toDestroy.Count != 0)
            {
                foreach (var chunk in _toDestroy)
                {
                    ReturnChunkToPool(chunk);
                }
            }

            //_curChunkPos = _currentChunk.transform.position;

            //Iterate through all positions within range, if that position doesn't have a chunk then set one there
            for (var x = _curChunkPos.x - _distance; x <= _curChunkPos.x + _distance; x += _chunkSize)
            {
                for (var y = _curChunkPos.y - _height; y <= _curChunkPos.y + _height; y += _chunkHeight)
                {
                    for (var z = _curChunkPos.z - _distance; z <= _curChunkPos.z + _distance; z += _chunkSize)
                    {
                        if (_chunkPool.Any(c => c.CurrentChunk == _world.Chunks[ChunkId.FromWorldPos(x, y, z)]))
                            continue;
                        //if (_world.Chunks.ContainsKey(ChunkId.FromWorldPos(x, y, z))) continue;
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

            for (var x = _start.x - _distance; x <= _start.x + _distance; x += _chunkSize)
            {
                //splits y and z for each row into a separate method, seemed to be faster
                GenerateRow(x);
                timeElapsed += Time.deltaTime;
            }
            //Uncomment debug log if you wish to see how fast it is (spoiler alert, Sonic speed)
            //Debug.Log("Time taken: " + timeElapsed);

            //World generation complete
            _chunksloaded = true;

            //Voxel Count display
            var voxelCount = _chunkPool.Count * (Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkHeight);
            Debug.Log("Voxel Count: " + voxelCount);
        }

        //Method for each x row, generate all of the y and z parts
        private void GenerateRow(float x)
        {
            for (var y = _start.y - _height; y <= _start.y + _height; y += _chunkHeight)
            {
                for (var z = _start.z - _distance; z <= _start.z + _distance; z += _chunkSize)
                {
                    CreateNewChunkObject(x, y, z);
                }
            }
        }

        private const float dataDist = 96f;
        public bool taskComplete;
        public int chunkCount;
        

        private IEnumerator GenerateWorldData()
        {
            StartCoroutine(ChunkLoop());

            while (!taskComplete)
            {
                yield return null;
            }

            GenerateWorld();
        }

        private IEnumerator ChunkLoop()
        {
            for (float x = _start.x - dataDist; x <= _start.x + dataDist; x+=_chunkSize)
            {
                for (float y = _start.y - dataDist; y <= _start.y + dataDist; y+=_chunkHeight)
                {
                    for (float z = _start.z - dataDist; z <= _start.z + dataDist; z+=_chunkSize)
                    {
                        GenerateChunkData(x, y, z);
                        yield return null;
                    }
                }
            }

            taskComplete = true;
            _progress.gameObject.SetActive(false);
        }

        private void GenerateChunkData(float x, float y, float z)
        {
            var total = ((dataDist * 2 + _chunkSize) * (dataDist * 2 + _chunkSize) * (dataDist * 2 + _chunkHeight)) / (_chunkSize * _chunkSize * _chunkHeight);
            _progress.value = (_world.Chunks.Count / total) * 100;
            if (_world.Chunks.ContainsKey(ChunkId.FromWorldPos(x, y, z))) return;
            var chunk = new Chunk(x, y, z, _voxelSize, this);
            _world.Chunks.Add(new ChunkId(x, y, z), chunk);
            //chunk.SetVoxel(x, y, z, _voxelSize);
            chunkCount++;
            var t = new Task(() => chunk.SetVoxel(x, y, z, _voxelSize));
            t.Start();
        }

        //Instantiate a new chunk
        private MonoChunk CreateNewChunkObject(float x, float y, float z)
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
            var chunk = chunkGameObject.GetComponent<MonoChunk>();

            chunk.IsAvailable = false;
            _chunkPool.Add(chunk);
            chunkGameObject.GetComponent<MeshRenderer>().material = _material;
            var newChunk = _world.Chunks[ChunkId.FromWorldPos(x, y, z)];
            chunk.UpdateChunk(newChunk);

            return chunk;
        }

        private void ReturnChunkToPool(MonoChunk target) => target.IsAvailable = true;

        private MonoChunk GetChunkObject(float X, float Y, float Z)
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
            var newChunk = _world.Chunks[ChunkId.FromWorldPos(x, y, z)];
            chunk.MeshRender.material = _material;
            
            chunk.UpdateChunk(newChunk);
        }
    }
}
