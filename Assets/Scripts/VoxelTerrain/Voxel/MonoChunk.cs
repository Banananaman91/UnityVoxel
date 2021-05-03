﻿using System;
using System.Collections;
using UnityEngine;
using VoxelTerrain.Grid;
using VoxelTerrain.Voxel.Dependencies;

namespace VoxelTerrain.Voxel
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MonoChunk : MonoBehaviour
    {
        private VoxelEngine _engine => FindObjectOfType<VoxelEngine>();
        public MeshCollider MeshCollider => GetComponent<MeshCollider>();
        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        private Vector3 Position => new Vector3(transform.position.x, -(Chunk.ChunkHeight * _engine.ChunkInfo.VoxelSize) / 2, transform.position.z);

        private void Start()
        {
            if (_engine.UpdateWater) StartCoroutine(UpdateWater());
        }

        private void Update()
        {
            //If we are in range, then don't do anything
            if (_engine.WithinRange(Position)) return;
        
            //if we are not in range, then lets die
            _engine.RemoveChunkAt(Position);
        }

        private static int to1D(int x, int y, int z) =>
            ((z * Chunk.ChunkSize * Chunk.ChunkHeight) + (y * Chunk.ChunkSize) + x);

        private static Vector3 to3D(int idx)
        {
            int z = idx / (Chunk.ChunkSize * Chunk.ChunkHeight);
            idx -= (z * Chunk.ChunkSize * Chunk.ChunkHeight);
            int y = idx / Chunk.ChunkSize;
            int x = idx % Chunk.ChunkSize;
            return new Vector3(x, y, z);
        }

        private IEnumerator UpdateWater()
        {
            while (Application.isPlaying)
            {
                //For each voxel in array
                for (int i = 0; i < Chunk.ChunkSize * Chunk.ChunkHeight * Chunk.ChunkSize; i++)
                {
                    //Keep current position updated, chunk may move
                    var pos = transform.position;
                    
                    //convert loop to 3D position
                    var coord = to3D(i);

                    //get current position in world
                    var curPos = pos + coord;

                    //check datatype of voxel at this world position
                    var type = _engine.WorldData[curPos.x, curPos.y, curPos.z];

                    //If water, start checking if any nearby empty space exists
                    if (type == (byte) VoxelType.Water)
                    {
                        //Get the nearest chunk position for neighbour space
                        var nearestChunk = _engine.NearestChunk(new Vector3(curPos.x + 1, curPos.y, curPos.z));
                        //Generate key id for checking dictionary
                        var key = new ChunkId(nearestChunk.x, nearestChunk.y, nearestChunk.z);
                        //if dictionary contains a chunk here and that voxel is air
                        if (_engine.WorldData.Chunks.ContainsKey(key) && _engine.WorldData[curPos.x + 1, curPos.y, curPos.z] == (byte) VoxelType.Default)
                        {
                            //set the voxel to water
                            _engine.WorldData[curPos.x + 1, curPos.y, curPos.z] = (byte) VoxelType.Water;
                            //if this chunk isn't already in queue for mesh updating, add it
                            if (!_engine._waterPool.ContainsKey(key)) _engine._waterPool.Add(key, _engine.WorldData.Chunks[key]);
                        }

                        //Get nearest chunk position for neighbour space
                        nearestChunk = _engine.NearestChunk(new Vector3(curPos.x, curPos.y, curPos.z + 1));
                        //Generate key id for checking dictionary
                        key = new ChunkId(nearestChunk.x, nearestChunk.y, nearestChunk.z);
                        //if dictionary contains a chunk here and that voxel is air
                        if (_engine.WorldData.Chunks.ContainsKey(key) && _engine.WorldData[curPos.x, curPos.y, curPos.z + 1] == (byte) VoxelType.Default)
                        {
                            //Set the voxel to water
                            _engine.WorldData[curPos.x, curPos.y, curPos.z + 1] = (byte) VoxelType.Water;
                            //if this chunk isn't already in queue for mesh updating, add it
                            if (!_engine._waterPool.ContainsKey(key)) _engine._waterPool.Add(key, _engine.WorldData.Chunks[key]);
                        }

                        //Get nearest chunk position for neighbour space
                        nearestChunk = _engine.NearestChunk(new Vector3(curPos.x - 1, curPos.y, curPos.z));
                        //generate key id for checking dictionary
                        key = new ChunkId(nearestChunk.x, nearestChunk.y, nearestChunk.z);
                        //if dictionary contains a chunk here and that voxel is air
                        if (_engine.WorldData.Chunks.ContainsKey(key) && _engine.WorldData[curPos.x - 1, curPos.y, curPos.z] == (byte) VoxelType.Default)
                        {
                            //set the voxel to water
                            _engine.WorldData[curPos.x - 1, curPos.y, curPos.z] = (byte) VoxelType.Water;
                            //if this chunk isn't already in queue for mesh updating, add it
                            if (!_engine._waterPool.ContainsKey(key)) _engine._waterPool.Add(key, _engine.WorldData.Chunks[key]);
                        }
                        
                        //Get nearest chunk position for neighbour space
                        nearestChunk = _engine.NearestChunk(new Vector3(curPos.x, curPos.y, curPos.z - 1));
                        //generate key id for checking dictionary
                        key = new ChunkId(nearestChunk.x, nearestChunk.y, nearestChunk.z);
                        //if dictionary contains a chunk here and that voxel is air
                        if (_engine.WorldData.Chunks.ContainsKey(key) && _engine.WorldData[curPos.x, curPos.y, curPos.z - 1] == (byte) VoxelType.Default)
                        {
                            //set the voxel to water
                            _engine.WorldData[curPos.x + i, curPos.y, curPos.z - 1] = (byte) VoxelType.Water;
                            //if this chunk isn't already in queue for mesh updating, add it
                            if (!_engine._waterPool.ContainsKey(key)) _engine._waterPool.Add(key, _engine.WorldData.Chunks[key]);
                        }
                    }

                    yield return null;
                }
            }
        }
    }
}
