﻿using System;
using System.Collections;
using UnityEngine;
using VoxelTerrain.Engine;
using VoxelTerrain.Engine.Dependencies;
using VoxelTerrain.Mouse;

namespace VoxelTerrain.Interactions
{
    [RequireComponent(typeof(VoxelInteraction))]
    public class HeadquartersPlacer : MonoBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private VoxelEngine _engine;
        [SerializeField] private VoxelInteraction _interaction;
#pragma warning restore 0649

        private void Awake()
        {
             StartCoroutine(SpawnHeadquarters());
        }

        private IEnumerator SpawnHeadquarters()
        {
            var chunkPoint = Vector3.zero;

            chunkPoint = _engine.NearestChunk(chunkPoint);

            Chunk chunk;
            var chunkId = new ChunkId(chunkPoint.x, chunkPoint.y, chunkPoint.z);

            //Wait until the chunk is generated
            //Do not force load
            do
            {
                chunk = _engine.ChunkAt(chunkId, false);
                yield return null;
            } while (chunk == null);
            
            //Set both x and z positions to the centre of the chunk
            var xz = Chunk.ChunkSize / 2;
            
            int yCounter = 0;

            //get the current voxel at the centre, lowest point
            var voxel = chunk[xz, yCounter, xz].Type;

            //continuously move up until we find an empty space to set the object position
            while (voxel != 0)
            {
                yCounter++;
                voxel = chunk[xz, yCounter, xz].Type;
                yield return null;
            }

            //set position
            transform.position = new Vector3(xz,chunkPoint.y + yCounter, xz);
            var pos = transform.position;
            pos.y -= 1;

            //set area around the placed object to voxel type, clear the space.
            StartCoroutine(_interaction.UpdateChunks(pos));
        }
    }
}
