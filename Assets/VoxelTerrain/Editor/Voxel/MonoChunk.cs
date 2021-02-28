using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelTerrain;
using VoxelTerrain.Dependencies;
using VoxelTerrain.Editor.Voxel;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MonoChunk : MonoBehaviour
{
    [HideInInspector] public VoxelEngine Engine;
    public MeshCollider MeshCollider => GetComponent<MeshCollider>();
    public MeshFilter MeshFilter => GetComponent<MeshFilter>();
    public MeshRenderer MeshRender => GetComponent<MeshRenderer>();
    public bool IsAvailable { get; set; }

    public Vector3 Position => new Vector3(transform.position.x, 0, transform.position.z);
    // Start is called before the first frame update
    void Awake()
    {
        Engine = FindObjectOfType<VoxelEngine>();
    }

    private void Update()
    {
        if (Engine.WithinRange(Position)) return;
        
        Engine.RemoveChunkAt(Position);
    }
}
