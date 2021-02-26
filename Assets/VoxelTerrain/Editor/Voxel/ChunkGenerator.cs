using System.Collections.Generic;
using MMesh;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.Dependencies;
using VoxelTerrain.Editor.Voxel.Jobs;

namespace VoxelTerrain.Editor.Voxel
{
    public class ChunkGenerator : MonoBehaviour
    {
        public VoxelEngine engine { get; set; }

        struct JobHolder
        {
            public ChunkVoxelSetter job;
            public JobHandle handle;
            public float startTime;
        }

        private Dictionary<Vector3, JobHolder> _jobs = new Dictionary<Vector3, JobHolder>();

        public Chunk CreateChunkJob(Vector3 worldOrigin)
        {
            if (!_jobs.ContainsKey(worldOrigin))
            {
                var chunk = LoadChunkAt(worldOrigin);
                
                var job = chunk.CreateJob(worldOrigin);
                var handle = job.Schedule();

                if (handle.IsCompleted)
                {
                    handle.Complete();

                    chunk.VoxelsFromJob(job);
                    
                    var go = new GameObject();

                    go.transform.position = worldOrigin;
                    go.name = worldOrigin.ToString();
                    go.AddComponent<MonoChunk>();
                    
                    chunk.AddEntity(go);
                    
                    chunk.SetMesh(worldOrigin);

                    _jobs.Remove(worldOrigin);

                    return chunk;
                }

                var holder = new JobHolder()
                {
                    job = job,
                    handle = handle,
                    startTime = Time.time
                };
                
                _jobs.Add(worldOrigin, holder);
            }

            else
            {
                var holder = _jobs[worldOrigin];
                var chunk = LoadChunkAt(worldOrigin);

                if (holder.handle.IsCompleted)
                {
                    holder.handle.Complete();
                    
                    chunk.VoxelsFromJob(holder.job);

                    var go = new GameObject();

                    go.transform.position = worldOrigin;
                    go.name = worldOrigin.ToString();
                    go.AddComponent<MonoChunk>();
                    
                    chunk.AddEntity(go);
                    
                    chunk.SetMesh(worldOrigin);

                    _jobs.Remove(worldOrigin);

                    return chunk;
                }
            }

            return null;
        }
        
        private Chunk LoadChunkAt(Vector3 worldOrigin)
        {
            if (engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z))) 
                return engine.WorldData.Chunks[ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z)];
            
            
            var chunk = new Chunk(worldOrigin.x, worldOrigin.y, worldOrigin.z, engine.ChunkInfo.VoxelSize, engine);
            engine.WorldData.Chunks.Add(new ChunkId(worldOrigin.x, worldOrigin.y, worldOrigin.z), chunk);
            return chunk;
        }

        private void OnDestroy()
        {
            foreach (var key in _jobs.Keys)
            {
                var holder = _jobs[key];
                holder.handle.Complete();
                holder.job.voxels.Dispose();
            }
        }
    }
}
