using System.Collections.Generic;
using MMesh;
using Unity.Collections;
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
                
                var job = CreateJob(worldOrigin);
                var handle = job.Schedule();

                if (handle.IsCompleted)
                {
                    handle.Complete();

                    chunk.VoxelsFromJob(job);

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

                    _jobs.Remove(worldOrigin);

                    return chunk;
                }
            }

            return null;
        }
        
        private Chunk LoadChunkAt(Vector3 worldOrigin) => engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z)) ? engine.WorldData.Chunks[ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z)] : new Chunk(worldOrigin.x, worldOrigin.y, worldOrigin.z, engine.ChunkInfo.VoxelSize, engine);
        
        private void OnDestroy()
        {
            foreach (var key in _jobs.Keys)
            {
                var holder = _jobs[key];
                holder.handle.Complete();
                holder.job.voxels.Dispose();
            }
        }
        
        public ChunkVoxelSetter CreateJob(Vector3 origin)
        {
            float resolution = engine.ChunkInfo.VoxelSize;
            float scale = engine.NoiseValues.SimplexOneScale;

            return new ChunkVoxelSetter
            {
                size = Chunk.ChunkSize,
                height = Chunk.ChunkHeight,
                heightMultiplier = 1,
                scale = scale,
                resolution = resolution,
                origin = origin,
                voxels = new NativeArray<float>((Chunk.ChunkSize + 1) * (Chunk.ChunkHeight + 1) * (Chunk.ChunkSize + 1), Allocator.Persistent),
                seed = 1
            };
        }
    }
}
