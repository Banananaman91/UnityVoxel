using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.Voxel.Dependencies;
using VoxelTerrain.Voxel.Jobs;

namespace VoxelTerrain.Voxel
{
    public class ChunkGenerator : MonoBehaviour
    {
        public VoxelEngine Engine { get; set; }

        struct JobHolder
        {
            public ChunkVoxelSetter Job;
            public JobHandle Handle;
            public float StartTime;
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
                    Job = job,
                    Handle = handle,
                    StartTime = Time.time
                };
                
                _jobs.Add(worldOrigin, holder);
            }

            else
            {
                var holder = _jobs[worldOrigin];
                var chunk = LoadChunkAt(worldOrigin);

                if (holder.Handle.IsCompleted)
                {
                    holder.Handle.Complete();
                    
                    chunk.VoxelsFromJob(holder.Job);

                    _jobs.Remove(worldOrigin);

                    return chunk;
                }
            }

            return null;
        }
        
        private Chunk LoadChunkAt(Vector3 worldOrigin) => Engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z)) ? Engine.WorldData.Chunks[ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z)] : new Chunk(worldOrigin.x, worldOrigin.y, worldOrigin.z, Engine.ChunkInfo.VoxelSize, Engine);
        
        private void OnDestroy()
        {
            foreach (var key in _jobs.Keys)
            {
                var holder = _jobs[key];
                holder.Handle.Complete();
                holder.Job.voxels.Dispose();
            }
        }

        private ChunkVoxelSetter CreateJob(Vector3 origin)
        {
            var resolution = Engine.ChunkInfo.VoxelSize;
            var scale = Engine.NoiseScale;
            var stoneDepth = Engine.VoxelTypeHeights.StoneDepth;
            var snowHeight = Engine.VoxelTypeHeights.SnowHeight;
            var caveStartHeight = Engine.VoxelTypeHeights.CaveStartHeight;
            var groundLevel = Engine.WorldInfo.GroundLevel;

            return new ChunkVoxelSetter
            {
                size = Chunk.ChunkSize,
                height = Chunk.ChunkHeight,
                scale = scale,
                resolution = resolution,
                origin = origin,
                voxels = new NativeArray<float>((Chunk.ChunkSize + 1) * (Chunk.ChunkHeight + 1) * (Chunk.ChunkSize + 1), Allocator.Persistent),
                seed = Engine.Seed,
                StoneDepth = stoneDepth,
                SnowHeight = snowHeight,
                CaveStartHeight = caveStartHeight,
                groundLevel = groundLevel
            };
        }
    }
}
