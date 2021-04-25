using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using VoxelTerrain.Voxel;
using VoxelTerrain.Voxel.Dependencies;

namespace VoxelTerrain.SaveLoad
{
    public class ChunkLoader : MonoBehaviour
    {
        [SerializeField] private string _chunkDirectoryName = "world";
        [SerializeField] private bool _enableSaving;
        [SerializeField] private bool _enableActiveWorld;
        private string _chunkDirectory;

        public string ChunkDirectoryName
        {
            get => _chunkDirectoryName;
            set => _chunkDirectoryName = value;
        }

        public void SetDirectoryName(string directoryName)
        {
            ChunkDirectoryName = directoryName;
        }

        private void Awake()
        {
            if (_enableActiveWorld)
            {
                var activeWorldDirectory = Application.persistentDataPath + "/" + "Active_World" + "/";

                if (Directory.Exists(activeWorldDirectory))
                {
                    var fullPath = activeWorldDirectory + "activeWorld" + ".json";

                    var fileContents = File.ReadAllText(fullPath);

                    _chunkDirectoryName = fileContents;
                }
            }

            _chunkDirectory = Application.persistentDataPath + "/" + "Worlds" + "/" + _chunkDirectoryName + "/";

            if (!Directory.Exists(_chunkDirectory)) Directory.CreateDirectory(_chunkDirectory);
        }

        public Chunk LoadChunkAt(Vector3 worldOrigin)
        {
            var fileName = worldOrigin.ToString();

            var fullPath = _chunkDirectory + fileName + ".json";
            if (!File.Exists(fullPath)) return null;

            var fileContents = File.ReadAllText(fullPath);

            var chunk = JsonConvert.DeserializeObject<Chunk>(fileContents);

            if (chunk.Voxels.Length != 0) return chunk;
            File.Delete(fullPath);
            return null;

        }

        public void SaveChunk(Chunk chunk, ChunkId chunkId)
        {
            if (!_enableSaving) return;

            if (chunk.Voxels.Length == 0) return;

            var worldOrigin = new Vector3(chunkId.X, chunkId.Y, chunkId.Z);

            var fileName = worldOrigin.ToString();

            var fullPath = _chunkDirectory + fileName + ".json";

            var json = JsonConvert.SerializeObject(chunk);
            
            File.WriteAllText(fullPath, json);
        }
    }
}
