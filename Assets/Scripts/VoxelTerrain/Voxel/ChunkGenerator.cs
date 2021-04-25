using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using VoxelTerrain.SaveLoad;
using VoxelTerrain.Voxel.Dependencies;
using VoxelTerrain.Voxel.Jobs;

namespace VoxelTerrain.Voxel
{
    public class ChunkGenerator : MonoBehaviour
    {
        [SerializeField] private ChunkLoader _chunkLoader;
        public VoxelEngine Engine { get; set; }

        struct JobHolder
        {
            public ChunkVoxelSetter Job;
            public JobHandle Handle;
            public float StartTime;
        }

        private Dictionary<Vector3, JobHolder> _jobs = new Dictionary<Vector3, JobHolder>();

        //Create jobs to run that will create all of the chunk data
        public Chunk CreateChunkJob(Vector3 worldOrigin)
        {
            if (!_jobs.ContainsKey(worldOrigin))
            {
                //If we don't have a job set up, lets load in chunk data
                var chunk = LoadChunkAt(worldOrigin);
                
                //Create a job for the current chunk data
                var job = CreateJob(worldOrigin);
                JobHandle handle;
                try
                {
                    handle = job.Schedule();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }

                //if it completed hella quick, then lets complete
                if (handle.IsCompleted)
                {
                    handle.Complete();

                    //set voxel data and dispose of job
                    chunk.Voxels = job.voxels.ToArray();
                    job.voxels.Dispose();

                    //remove job from the dictionary, it isn't needed anymore
                    _jobs.Remove(worldOrigin);

                    return chunk;
                }

                //otherwise, lets create a holder and add it to the dictionary
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
                //If we already have a job, load in the chunk data and check job is complete.
                //If so, set data and remove job from dictionary
                var holder = _jobs[worldOrigin];
                var chunk = LoadChunkAt(worldOrigin);

                if (holder.Handle.IsCompleted)
                {
                    holder.Handle.Complete();
                    
                    chunk.Voxels = holder.Job.voxels.ToArray();
                    holder.Job.voxels.Dispose();

                    _jobs.Remove(worldOrigin);

                    return chunk;
                }
            }

            return null;
        }
        
        //Check if the world data already contains the chunk, if not create one
        private Chunk LoadChunkAt(Vector3 worldOrigin) => Engine.WorldData.Chunks.ContainsKey(ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z)) ? Engine.WorldData.Chunks[ChunkId.FromWorldPos(worldOrigin.x, worldOrigin.y, worldOrigin.z)] : new Chunk(Engine);
        
        private void OnDestroy()
        {
            foreach (var key in _jobs.Keys)
            {
                var holder = _jobs[key];
                holder.Handle.Complete();
                holder.Job.voxels.Dispose();
            }
        }

        //create chunk job for setting voxel data with noise values
        private ChunkVoxelSetter CreateJob(Vector3 origin)
        {
            var resolution = Engine.ChunkInfo.VoxelSize;
            var scale = Engine.NoiseScale;
            var groundLevel = Engine.WorldInfo.GroundLevel;
            var seed = Engine.WorldInfo.Seed;

            return new ChunkVoxelSetter
            {
                size = Chunk.ChunkSize,
                height = Chunk.ChunkHeight,
                scale = scale,
                resolution = resolution,
                origin = origin,
                voxels = new NativeArray<byte>((Chunk.ChunkSize) * (Chunk.ChunkHeight) * (Chunk.ChunkSize), Allocator.Persistent),
                seed = seed,
                groundLevel = groundLevel,
            };
        }
    }
}
