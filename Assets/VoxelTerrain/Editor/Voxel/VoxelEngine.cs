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

        private void SpawnChunk(Chunk nonNullChunk, Vector3 pos)
        {
            var chunkId = new ChunkId(pos.x, pos.y, pos.z);
            WorldData.Chunks.Add(chunkId, nonNullChunk);

            var go = Instantiate(_chunkInfo.ChunkPrefab.gameObject);

            go.transform.position = pos;
            go.name = pos.ToString();

            nonNullChunk.AddEntity(go);
                    
            nonNullChunk.SetMesh(pos);
            
            WorldData.ChunkObjects.Add(chunkId, go);
        }

        private void Update()
        {
            var point = NearestChunk(Position);

            for (var x = -_worldInfo.Distance; x <= _worldInfo.Distance; x += Chunk.ChunkSize)
            {
                for (var z = -_worldInfo.Distance; z <= _worldInfo.Distance; z += Chunk.ChunkSize)
                {
                    var pointToCheck = new ChunkId(point.x + x, 0, point.z + z);

                    var c = ChunkAt(pointToCheck, false);

                    if (c == null)
                    {
                        c = LoadChunkAt(pointToCheck);
                        
                        if (c != null) SpawnChunk(c, new Vector3(point.x + x, 0, point.z + z));
                    }
                }
            }
        }
        #endregion

        #region Chunk Object Handling

        private void ReturnChunkToPool(MonoChunk target) => target.IsAvailable = true;
        
        #endregion
    }
}
