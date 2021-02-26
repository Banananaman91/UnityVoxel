using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using VoxelTerrain.Dependencies;
using VoxelTerrain.Editor.Mouse;
using VoxelTerrain.Editor.Noise;
using VoxelTerrain.Editor.Voxel.Dependencies;
using VoxelTerrain.Editor.Voxel.InfoData;

namespace VoxelTerrain.Editor.Voxel
{
    [RequireComponent(typeof(WorldInfo), typeof(ChunkInfo), typeof(VoxelTypeHeights))]
    [RequireComponent(typeof(NoiseInfo), typeof(NoiseValues), typeof(CellularInfo))]
    [RequireComponent(typeof(DomainWarpInfo), typeof(FractalInfo), typeof(ProgressBar))]
    public class VoxelEngine : MonoBehaviour
    {
        public FastNoiseLite _fastNoise = new FastNoiseLite();
        public World WorldData = new World();
        public Dictionary<ChunkId, MonoChunk> _chunkPoolDictionary = new Dictionary<ChunkId, MonoChunk>();
        
        [SerializeField] private WorldInfo _worldInfo;
        [SerializeField] private ChunkInfo _chunkInfo;
        [SerializeField] private NoiseInfo _noiseInfo;
        [SerializeField] private FractalInfo _fractalInfo;
        [SerializeField] private CellularInfo _cellularInfo;
        [SerializeField] private DomainWarpInfo _domainWarpInfo;
        [SerializeField] private VoxelTypeHeights _voxelTypeHeights;
        [SerializeField] private NoiseValues _noiseValues;
        [SerializeField] private ProgressBar _progressBar;
        [SerializeField] private MousePositionDraw _mousePositionDraw;
        [SerializeField] private WorldGenerationFunctions _worldGeneration;

        private Vector3 _start;
        private Vector3 _curChunkPos;
        private List<MonoChunk> _toDestroy = new List<MonoChunk>();
        private List<Task> _taskPool = new List<Task>();
        private MonoChunk _currentChunk { get; set; }
        private bool _chunksloaded;
        private bool _worldLoaded;

        private Vector3 Position => _worldInfo.Origin != null ? _worldInfo.Origin.position : Vector3.zero;
        public ChunkInfo ChunkInfo => _chunkInfo;
        public float ChunkSize => 16 * _chunkInfo.VoxelSize;
        public float ChunkHeight => 32 * _chunkInfo.VoxelSize;
        public NoiseValues NoiseValues => _noiseValues;
        public VoxelTypeHeights VoxelTypeHeights => _voxelTypeHeights;

        public bool UsePerlin;
        
        // #region Validate Function
        // #if UNITY_EDITOR || UNITY_64
        // private void OnValidate()
        // {
        //     if (_noiseInfo.RandomSeed || _noiseInfo.SetCustomSeed)
        //     {
        //         if (_noiseInfo.RandomSeedActive && _noiseInfo.SetCustomSeed)
        //         {
        //             _noiseInfo.RandomSeed = false;
        //             _noiseInfo.RandomSeedActive = false;
        //             _noiseInfo.CustomSeedActive = true;
        //         }
        //         else if (_noiseInfo.CustomSeedActive && _noiseInfo.RandomSeed)
        //         {
        //             _noiseInfo.SetCustomSeed = false;
        //             _noiseInfo.CustomSeedActive = false;
        //             _noiseInfo.RandomSeedActive = true;
        //         }
        //         else if (!_noiseInfo.RandomSeedActive && _noiseInfo.RandomSeed)
        //         {
        //             _noiseInfo.RandomSeedActive = true;
        //         }
        //         else if (!_noiseInfo.CustomSeedActive && _noiseInfo.SetCustomSeed)
        //         {
        //             _noiseInfo.CustomSeedActive = true;
        //         }
        //     }
        // }
        // #endif
        // #endregion

        #region Unity Functions
        private void Awake()
        {
            _start = transform.position;
            if (_noiseInfo.RandomSeed) _fastNoise.SetSeed(UnityEngine.Random.Range(0, 9999));
            else if (_noiseInfo.SetCustomSeed) _fastNoise.SetSeed(_noiseInfo.SeedValue);
            _fastNoise.SetNoiseType(_noiseInfo.NoiseType);
            _fastNoise.SetFrequency(_noiseInfo.NoiseFrequency);
            _fastNoise.SetRotationType3D(_noiseInfo.RotationType3D);
            _fastNoise.SetFractalType(_fractalInfo.FractalType);
            _fastNoise.SetFractalOctaves(_fractalInfo.Octaves);
            _fastNoise.SetFractalLacunarity(_fractalInfo.Lacunarity);
            _fastNoise.SetFractalGain(_fractalInfo.Gain);
            _fastNoise.SetFractalWeightedStrength(_fractalInfo.WeightedStrength);
            _fastNoise.SetFractalPingPongStrength(_fractalInfo.PingPongStrength);
            _fastNoise.SetCellularDistanceFunction(_cellularInfo.CellularDistanceFunction);
            _fastNoise.SetCellularReturnType(_cellularInfo.CellularReturnType);
            _fastNoise.SetCellularJitter(_cellularInfo.CellularJitterModifier);
            _fastNoise.SetDomainWarpType(_domainWarpInfo.DomainWarpType);
            _fastNoise.SetDomainWarpAmp(_domainWarpInfo.DomainWarpAmp);
            //StartCoroutine(GenerateWorldData()); 
            _worldGeneration.GenerateWorld(_start, _worldInfo.Distance, _chunkInfo.VoxelSize);
        }

        private Vector3 NearestChunk(Vector3 position)
        {
            var curChunkPosX = Mathf.FloorToInt(Position.x / ChunkSize) * ChunkSize;
            var curChunkPosY = Mathf.FloorToInt(Position.y / ChunkHeight) * ChunkHeight;
            var curChunkPosZ = Mathf.FloorToInt(Position.z / ChunkSize) * ChunkSize;

            return new Vector3(curChunkPosX, curChunkPosY, curChunkPosZ);
        }

        private Chunk ChunkAt(ChunkId point, bool forceLoad = true)
        {
            if (WorldData.Chunks.ContainsKey(point)) return WorldData.Chunks[point];
            if (!forceLoad) return null;

            return LoadChunkAt(point);
        }

        private Chunk LoadChunkAt(ChunkId point)
        {
            var x = point.X;
            var z = point.Z;

            var origin = new Vector3(x, 0, z);

            return _worldGeneration.ChunkGenerator.CreateChunkJob(origin);
        }

        private void Update()
        {
            var point = NearestChunk(Position);

            for (var x = -_worldInfo.Distance; x <= _worldInfo.Distance; x += Chunk.ChunkSize)
            {
                for (var z = -_worldInfo.Distance; z <= _worldInfo.Distance; z += Chunk.ChunkSize)
                {
                    var pointToCheck = new ChunkId(point.x + x, 0, point.z + z);

                    var c = ChunkAt(pointToCheck);

                    if (c == null) LoadChunkAt(pointToCheck);
                }
            }

            // if (!_chunksloaded) return; // Don't run until world generation is complete
            //
            // //Get current position from origin
            // var curChunkPosX = Mathf.FloorToInt(Position.x / ChunkSize) * ChunkSize;
            // var curChunkPosY = Mathf.FloorToInt(Position.y / ChunkHeight) * ChunkHeight;
            // var curChunkPosZ = Mathf.FloorToInt(Position.z / ChunkSize) * ChunkSize;
            //
            // //check if a chunk exists in current position
            // var hasChunk =
            //     _chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ));
            // if (!hasChunk) return;
            //
            // //If position has chunk, get chunky boi
            // var chunk = _chunkPoolDictionary[ChunkId.FromWorldPos(curChunkPosX, curChunkPosY, curChunkPosZ)];
            //
            // //Set current chunk if one hasn't already been set
            // if (_currentChunk == null) _currentChunk = chunk;
            //
            // //Chunk comparison, update terrain if current chunk doesn't match
            // if (chunk == _currentChunk) return;
            // _currentChunk = chunk;
            // _curChunkPos = new Vector3(curChunkPosX, curChunkPosY, curChunkPosZ);
            // ExpandTerrain();
        }
        #endregion

        #region Expand Terrain
        private void ExpandTerrain()
        {
            _toDestroy.Clear();

            //Collect all chunks that are out of range to be 'destroyed'
            foreach (var chunk in _chunkPoolDictionary.Values)
            {
                if (Mathf.Abs(_curChunkPos.x - chunk.transform.position.x) > _worldInfo.Distance ||
                    Mathf.Abs(_curChunkPos.y - chunk.transform.position.y) > _worldInfo.Height ||
                    Mathf.Abs(_curChunkPos.z - chunk.transform.position.z) > _worldInfo.Distance)
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
            for (var x = _curChunkPos.x - _worldInfo.Distance; x <= _curChunkPos.x + _worldInfo.Distance; x += ChunkSize)
            {
                for (var y = _curChunkPos.y - _worldInfo.Height; y <= _curChunkPos.y + _worldInfo.Height; y += ChunkHeight)
                {
                    for (var z = _curChunkPos.z - _worldInfo.Distance; z <= _curChunkPos.z + _worldInfo.Distance; z += ChunkSize)
                    {
                        if (_chunkPoolDictionary.ContainsKey(ChunkId.FromWorldPos(x, y, z))) continue;
                        BuildChunk(x, y, z);
                    }
                }
            }

            //yield return null;
        }
        
        #endregion

        #region World Generation
        //Build the world, the whole world, in his hands
        private void GenerateWorld()
        {
            for (var x = _start.x - _worldInfo.Distance; x <= _start.x + _worldInfo.Distance; x += ChunkSize)
            {
                //splits y and z for each row into a separate method, seemed to be faster
                GenerateRow(x);
            }
            //Uncomment debug log if you wish to see how fast it is (spoiler alert, Sonic speed)
            //Debug.Log("Time taken: " + timeElapsed);

            //World generation complete
            _chunksloaded = true;
            

            //Voxel Count display
            var voxelCount = _chunkPoolDictionary.Count * (Chunk.ChunkSize * Chunk.ChunkSize * Chunk.ChunkHeight);
            Debug.Log("Voxel Count: " + voxelCount);
        }

        //Method for each x row, generate all of the y and z parts
        private void GenerateRow(float x)
        {
            for (var z = _start.z - _worldInfo.Distance; z <= _start.z + _worldInfo.Distance; z += ChunkSize)
            {
                CreateNewChunkObject(x, 0, z);
            }
        }

        public const float DataDist = 0;
        public const float DataHeight = 0;

        private IEnumerator GenerateWorldData()
        {
            _progressBar.ProgressText.text = "Generating World Chunks";

            for (float x = _start.x - DataDist; x <= _start.x + DataDist; x += ChunkSize)
            {
                for (float z = _start.z - DataDist; z <= _start.z + DataDist; z += ChunkSize)
                {
                    GenerateChunkData(x, 0, z);
                    yield return null;
                }
            }

            _progressBar.ProgressText.text = "Setting Voxel Data";

            // while (_taskPool.All(x => x.Status != TaskStatus.RanToCompletion))
            // {
            //     var taskCount = 0;
            //     for (int i = 0; i <= _taskPool.Count; i++)
            //     {
            //         if (_taskPool[i].Status == TaskStatus.RanToCompletion) taskCount++;
            //     }
            //
            //     _progressBar.Progress.value = taskCount / _taskPool.Count * 100;
            //     yield return null;
            // }

            _progressBar.Progress.gameObject.SetActive(false);
            GenerateWorld();
        }

        private void GenerateChunkData(float x, float y, float z)
        {
            var total = ((DataDist * 2 + ChunkSize) * (DataDist * 2 + ChunkSize) * (DataDist * 2 + ChunkHeight)) / (ChunkSize * ChunkSize * ChunkHeight);
            _progressBar.Progress.value = (WorldData.Chunks.Count / total) * 100;
            if (WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(x, y, z))) return;
            var chunk = new Chunk(x, y, z, _chunkInfo.VoxelSize, this);
            WorldData.Chunks.Add(new ChunkId(x, y, z), chunk);
            chunk.SetVoxel(x, y, z, _chunkInfo.VoxelSize);
            // var t = new Task(() => chunk.SetVoxel(x, y, z, _chunkInfo.VoxelSize));
            // _taskPool.Add(t);
            // t.Start();
        }

        #endregion

        #region Chunk Object Handling
        //Instantiate a new chunk
        private MonoChunk CreateNewChunkObject(float x, float y, float z)
        {
            //Uncomment next line if you wish to not use prefabs
            //var chunkGameObject = new GameObject("Chunk " + x + ", " + y + ", " + z);

            //Comment out next lines if you wish to not use prefabs
            var chunkGameObject = Instantiate(_chunkInfo.ChunkPrefab, new Vector3(x, y, z), Quaternion.identity);
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
            _chunkPoolDictionary.Add(new ChunkId(x,y,z), chunk);
            chunkGameObject.GetComponent<MeshRenderer>().material = _worldInfo.Material;
            var newChunk = WorldData.Chunks[ChunkId.FromWorldPos(x, y, z)];
            chunk.UpdateChunk(newChunk);

            return chunk;
        }

        private void ReturnChunkToPool(MonoChunk target) => target.IsAvailable = true;

        private MonoChunk GetChunkObject(float X, float Y, float Z)
        {
            var chunk = _chunkPoolDictionary.FirstOrDefault(x => x.Value.IsAvailable).Value;
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
            var hasChunk = WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(x, y, z));
            if (!hasChunk) return;
            var newChunk = WorldData.Chunks[ChunkId.FromWorldPos(x, y, z)];
            chunk.MeshRender.material = _worldInfo.Material;

            chunk.UpdateChunk(newChunk);
        }
        #endregion
    }
}
