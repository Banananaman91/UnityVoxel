﻿using System.Collections.Generic;
using TerrainData;
using Unity.Collections;
using UnityEngine;
using VoxelTerrain.Engine.InfoData;

namespace VoxelTerrain.Engine.Dependencies
{
    //If you are here, leave, you're not welcome
    public class World
    {
        public Dictionary<ChunkId, Chunk> Chunks = new Dictionary<ChunkId, Chunk>();
        public Dictionary<ChunkId, GameObject> ChunkObjects = new Dictionary<ChunkId, GameObject>();
        public VoxelEngine Engine { get; set; }

        public int this[float x, float y, float z]
        {
            get
            {
                var curChunkPosX = Mathf.FloorToInt(x / Chunk.ChunkSize) * Chunk.ChunkSize;
                var curChunkPosZ = Mathf.FloorToInt(z / Chunk.ChunkSize) * Chunk.ChunkSize;
                var chunkPos = new Vector3(curChunkPosX, -Chunk.ChunkHeight / 2, curChunkPosZ);
                if (!Chunks.ContainsKey(new ChunkId(chunkPos.x, chunkPos.y, chunkPos.z))) return 0;
                var chunk = Chunks[new ChunkId(chunkPos.x, chunkPos.y, chunkPos.z)];
                var voxPos = new Vector3(x, y, z) - chunkPos;
                return chunk[voxPos.x, voxPos.y, voxPos.z].Type;
            }

            set
            {
                var curChunkPosX = Mathf.FloorToInt(x / Chunk.ChunkSize) * Chunk.ChunkSize;
                var curChunkPosZ = Mathf.FloorToInt(z / Chunk.ChunkSize) * Chunk.ChunkSize;
                var chunkPos = new Vector3(curChunkPosX, -Chunk.ChunkHeight / 2, curChunkPosZ);
                if (!Chunks.ContainsKey(new ChunkId(chunkPos.x, chunkPos.y, chunkPos.z))) return;
                var chunk = Chunks[new ChunkId(chunkPos.x, chunkPos.y, chunkPos.z)];
                var voxPos = new Vector3(x, y, z) - chunkPos;
                var voxel = chunk[voxPos.x, voxPos.y, voxPos.z];
                voxel.Type = value;
            }
        }

        public Chunk GetChunkAt(Vector3 pos) => Chunks.ContainsKey(ChunkId.FromWorldPos(pos.x, pos.y, pos.z)) ? Chunks[ChunkId.FromWorldPos(pos.x, pos.y, pos.z)] : null;
        
        public Chunk GetNonNullChunkAt(Vector3 pos) => Chunks.ContainsKey(ChunkId.FromWorldPos(pos.x, pos.y, pos.z)) ? Chunks[ChunkId.FromWorldPos(pos.x, pos.y, pos.z)] : Engine.LoadChunkAt(new ChunkId(pos.x, pos.y, pos.z));
        
        public Engine.Voxel GetVoxelAt(float x, float y, float z, Vector3 chunkPos, float scale, Chunk currentChunk = null, Chunk rightChunk = null, Chunk forwardChunk = null, Chunk rightForwardChunk = null)
        {
            //var chunkPos = new Vector3(); //NearestChunk(new Vector3(x, y, z), scale);
            Chunk chunk = null;
            
            //Neighbour checking for chunks
            if (currentChunk != null && x != Chunk.ChunkSize && z != Chunk.ChunkSize) chunk = currentChunk;
            else if (rightChunk != null && x == Chunk.ChunkSize && z != Chunk.ChunkSize) chunk = rightChunk;
            else if (forwardChunk != null && x != Chunk.ChunkSize && z == Chunk.ChunkSize) chunk = forwardChunk;
            else if (rightForwardChunk != null && x == Chunk.ChunkSize && z == Chunk.ChunkSize) chunk = rightForwardChunk;

            if (chunk == null)
            {
                var noiseArray = new NativeArray<NoiseInfo>(Engine.NoiseInfo, Allocator.Persistent);
                var value = BiomeGenerator.GenerateVoxelType(chunkPos.x + x * scale, chunkPos.y + y * scale, chunkPos.z + z * scale, noiseArray, Engine.WorldInfo.Seed);
                noiseArray.Dispose();
                return value;
            }

            if (x >= Chunk.ChunkSize) x = 0;
            if (z >= Chunk.ChunkSize) z = 0;
            //var voxPos = (new Vector3(x, y, z) - chunkPos) / scale;
            return chunk[x, y, z];
        }
        
        private Vector3 NearestChunk(Vector3 pos, float scale)
        {
            var curChunkPosX = Mathf.FloorToInt(pos.x / (Chunk.ChunkSize * scale)) * (Chunk.ChunkSize * scale);
            var curChunkPosZ = Mathf.FloorToInt(pos.z / (Chunk.ChunkSize * scale)) * (Chunk.ChunkSize * scale);

            return new Vector3(curChunkPosX, -(Chunk.ChunkHeight * scale) / 2, curChunkPosZ);
        }
        
        /*
        private int SetVoxelType(float x, float y, float z)
        {
            var blockType = VoxelType.Default;

            // noise for heightmap
            var simplex1 = Noise.Generate2DNoiseValue( x, z, Engine.NoiseScale, Engine.WorldInfo.NumGen, Engine.WorldInfo.GroundLevel);

            //under the surface, dirt block
            if (y <= simplex1)
            {
                //blockType = VoxelType.Dirt;
                blockType = VoxelType.Dirt;
            
                //just on the surface, use a grass type
                if (y > simplex1 - 1)
                {
                    blockType = VoxelType.Grass;
                }
            }

            return (int) blockType;
        }
        */
    }
}
