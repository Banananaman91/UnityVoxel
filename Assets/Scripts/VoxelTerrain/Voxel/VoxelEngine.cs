using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using VoxelTerrain.Grid;
using VoxelTerrain.Voxel.Dependencies;
using VoxelTerrain.Voxel.InfoData;

namespace VoxelTerrain.Voxel
{
    [RequireComponent(typeof(WorldInfo), typeof(ChunkInfo), typeof(VoxelTypeHeights))]
    [RequireComponent(typeof(WorldGenerationFunctions))]
    public class VoxelEngine : MonoBehaviour
    {
        public World WorldData = new World();

        [SerializeField] private WorldInfo _worldInfo;
        [SerializeField] private ChunkInfo _chunkInfo;
        [SerializeField] private VoxelTypeHeights _voxelTypeHeights;
        [SerializeField] private WorldGenerationFunctions _worldGeneration;
        [SerializeField] private float _noiseScale;
        [SerializeField] private bool _updateWater;

        public bool UpdateWater => _updateWater;

        private float _maxMagnitude;
        
        //Water update pool
        public Dictionary<ChunkId, Chunk> _waterPool = new Dictionary<ChunkId, Chunk>();

        private Vector3 Position => _worldInfo.Origin != null ? new Vector3(_worldInfo.Origin.position.x, -ChunkHeight / 2, _worldInfo.Origin.position.z) : Vector3.zero;
        public ChunkInfo ChunkInfo => _chunkInfo;
        public float ChunkSize => Chunk.ChunkSize * _chunkInfo.VoxelSize;
        public float ChunkHeight => Chunk.ChunkHeight * _chunkInfo.VoxelSize;
        public VoxelTypeHeights VoxelTypeHeights => _voxelTypeHeights;
        public float NoiseScale => _noiseScale;

        public WorldInfo WorldInfo => _worldInfo;

        #region Unity Functions
        private void Awake()
        {
            var activeWorldDirectory = Application.persistentDataPath + "/" + "Active_World" + "/";

            if (Directory.Exists(activeWorldDirectory))
            {
                var fullPath = activeWorldDirectory + "activeWorld" + ".json";

                var fileContents = File.ReadAllText(fullPath);
                
                var directory = Application.persistentDataPath + "/" + "Worlds" + "/" + fileContents + "/";

                if (File.Exists(directory + "seed.json"))
                {
                    fullPath = directory + "seed.json";

                    fileContents = File.ReadAllText(fullPath);

                    WorldInfo.Seed = Convert.ToInt32(fileContents);
                }
            }

            WorldData.Engine = this;
            _worldGeneration.GenerateWorld(transform.position, _worldInfo.Distance,  - (ChunkHeight / 2), _chunkInfo.VoxelSize);
        }

        private void Start()
        {
            var corner = new Vector3(-_worldInfo.Distance, 0, -_worldInfo.Distance);
            _maxMagnitude = (Position - corner).magnitude;
        }

        private Dictionary<ChunkId, Chunk> _chunkPool = new Dictionary<ChunkId, Chunk>();

        private void FixedUpdate()
        {
            if (_chunkPool.Count > 0)
            {
                var chunk = _chunkPool.First();
                SpawnChunk(chunk.Value, new Vector3(chunk.Key.X, chunk.Key.Y, chunk.Key.Z));
                _chunkPool.Remove(chunk.Key);
            }

            if (_waterPool.Count > 0)
            {
                var chunk = _waterPool.First();
                chunk.Value.SetMesh(new Vector3(chunk.Key.X, chunk.Key.Y, chunk.Key.Z));
                _waterPool.Remove(chunk.Key);
            }
        }

        private void Update()
        {
            var point = NearestChunk(Position);

            for (var x = -_worldInfo.Distance; x <= _worldInfo.Distance; x += ChunkSize)
            {
                for (var z = -_worldInfo.Distance; z <= _worldInfo.Distance; z += ChunkSize)
                {
                    var pointToCheck = new ChunkId(point.x + x, -(ChunkHeight / 2), point.z + z);
                    
                    //Check chunk pool doesn't already have object
                    if (_chunkPool.ContainsKey(pointToCheck)) continue;
                    
                    //check position is within distance, rounds off view area.
                    if (Vector3.Distance(new Vector3(pointToCheck.X, -(ChunkHeight / 2), pointToCheck.Z), Position) >
                        _worldInfo.Distance) continue;

                    //check for chunk in the world data, in case it has already been spawned
                    var c = ChunkAt(pointToCheck, false);

                    //if chunk is not found, attempt to load one
                    //Update repeatedly checks until we have a chunk
                    if (c == null)
                    {
                        c = LoadChunkAt(pointToCheck);
                        
                        if (c != null) _chunkPool.Add(new ChunkId(point.x + x, -(ChunkHeight / 2), point.z + z), c);//  SpawnChunk(c, new Vector3(point.x + x, -(ChunkHeight / 2), point.z + z));
                    }
                }
            }
        }
        #endregion

        #region Voxel Methods
        //Convert position to the nearest chunk position
        public Vector3 NearestChunk(Vector3 pos)
        {
            var curChunkPosX = Mathf.FloorToInt(pos.x / ChunkSize) * ChunkSize;
            var curChunkPosZ = Mathf.FloorToInt(pos.z / ChunkSize) * ChunkSize;

            return new Vector3(curChunkPosX, -(ChunkHeight / 2), curChunkPosZ);
        }

        //Get the chunk at a current point. Use force load to make it return a chunk when there isn't one
        public Chunk ChunkAt(ChunkId point, bool forceLoad = true)
        {
            if (WorldData.Chunks.ContainsKey(point)) return WorldData.Chunks[point];
            if (!forceLoad) return null;

            return LoadChunkAt(point);
        }

        //Load chunk from file
        public Chunk LoadFromFile(Vector3 pos) => _worldGeneration.ChunkLoader.LoadChunkAt(pos);

        //Load chunk at current position
        public Chunk LoadChunkAt(ChunkId point)
        {
            var x = point.X;
            var z = point.Z;

            var origin = new Vector3(x, -(ChunkHeight / 2), z);
            
            return _worldGeneration.GenerateChunkData(origin);
        }

        //Spawn chunk gameobject for scene using chunk data
        private void SpawnChunk(Chunk nonNullChunk, Vector3 pos)
        {
            nonNullChunk.AddEngine(this);
            var chunkId = new ChunkId(pos.x, pos.y, pos.z);
            WorldData.Chunks.Add(chunkId, nonNullChunk);

            var go = Instantiate(_chunkInfo.ChunkPrefab.gameObject, pos, Quaternion.identity);

            //go.transform.position = pos;
            go.name = pos.ToString();

            nonNullChunk.AddEntity(go);
                    
            nonNullChunk.SetMesh(pos);
            
            if (WorldData.ChunkObjects.ContainsKey(chunkId)) Debug.Log("Chunk: " + chunkId.X + ", " + chunkId.Y + ", " + chunkId.Z + " Exists");
            WorldData.ChunkObjects.Add(chunkId, go);
        }
        
        //Remove the chunk at this position, both data and gameobject
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

        //Check if position is within range
        public bool WithinRange(Vector3 pos) => Vector3.Distance(Position, pos) <= WorldInfo.Distance;

        #endregion
    }
}
