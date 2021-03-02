using UnityEngine;
using VoxelTerrain.Editor.Voxel.Dependencies;
using VoxelTerrain.Editor.Voxel.InfoData;

namespace VoxelTerrain.Editor.Voxel
{
    [RequireComponent(typeof(WorldInfo), typeof(ChunkInfo), typeof(VoxelTypeHeights))]
    public class VoxelEngine : MonoBehaviour
    {
        public World WorldData = new World();

        [SerializeField] private WorldInfo _worldInfo;
        [SerializeField] private ChunkInfo _chunkInfo;
        [SerializeField] private VoxelTypeHeights _voxelTypeHeights;
        [SerializeField] private WorldGenerationFunctions _worldGeneration;
        [SerializeField] private float _noiseScale;
        
        private float _maxMagnitude;

        private Vector3 Position => _worldInfo.Origin != null ? new Vector3(_worldInfo.Origin.position.x, 0, _worldInfo.Origin.position.z) : Vector3.zero;
        public ChunkInfo ChunkInfo => _chunkInfo;
        private float ChunkSize => Chunk.ChunkSize * _chunkInfo.VoxelSize;
        public VoxelTypeHeights VoxelTypeHeights => _voxelTypeHeights;
        public float NoiseScale => _noiseScale;

        #region Unity Functions
        private void Awake()
        {
            _worldGeneration.GenerateWorld(transform.position, _worldInfo.Distance, _chunkInfo.VoxelSize);
        }

        private void Start()
        {
            var corner = new Vector3(-_worldInfo.Distance, 0, -_worldInfo.Distance);
            _maxMagnitude = (Position - corner).magnitude;
        }

        public Vector3 NearestChunk(Vector3 pos)
        {
            var curChunkPosX = Mathf.FloorToInt(pos.x / ChunkSize) * ChunkSize;
            var curChunkPosZ = Mathf.FloorToInt(pos.z / ChunkSize) * ChunkSize;

            return new Vector3(curChunkPosX, 0, curChunkPosZ);
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
        
        public void RemoveChunkAt(Vector3 pos)
        {
            var point = new ChunkId(pos.x, pos.y, pos.z);
            if (WorldData.ChunkObjects.ContainsKey(point))
            {
                var go = WorldData.ChunkObjects[point];
                Destroy(go);
                WorldData.ChunkObjects.Remove(point);
            }

            if (WorldData.Chunks.ContainsKey(point))
            {
                WorldData.Chunks.Remove(point);
            }
        }

        public bool WithinRange(Vector3 pos)
        {
            var difference = Position - pos;

            return difference.magnitude <= _maxMagnitude;
        }

        private void Update()
        {
            var point = NearestChunk(Position);

            for (var x = -_worldInfo.Distance; x < _worldInfo.Distance; x += ChunkSize)
            {
                for (var z = -_worldInfo.Distance; z < _worldInfo.Distance; z += ChunkSize)
                {
                    var pointToCheck = new ChunkId(point.x + x, 0, point.z + z);
                    if (Vector3.Distance(new Vector3(pointToCheck.X, 0, pointToCheck.Z), Position) >
                        _worldInfo.Distance) continue;

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
    }
}
