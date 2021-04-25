using System;
using UnityEngine;

namespace VoxelTerrain.Voxel
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MonoChunk : MonoBehaviour
    {
        private VoxelEngine _engine => FindObjectOfType<VoxelEngine>();
        public MeshCollider MeshCollider => GetComponent<MeshCollider>();
        public MeshFilter MeshFilter => GetComponent<MeshFilter>();
        private Vector3 Position => new Vector3(transform.position.x, -(Chunk.ChunkHeight * _engine.ChunkInfo.VoxelSize) / 2, transform.position.z);

        private void Update()
        {
            //If we are in range, then don't do anything
            if (_engine.WithinRange(Position)) return;
        
            //if we are not in range, then lets die
            _engine.RemoveChunkAt(Position);
        }
    }
}
