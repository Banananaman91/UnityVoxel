﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelTerrain;
using VoxelTerrain.Editor.Voxel;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MonoChunk : MonoBehaviour
{
    [HideInInspector] public VoxelEngine Engine;
    public MeshCollider MeshCollider => GetComponent<MeshCollider>();
    public MeshFilter MeshFilter => GetComponent<MeshFilter>();
    public MeshRenderer MeshRender => GetComponent<MeshRenderer>();
    public bool IsAvailable { get; set; }
    public bool MeshUpdate { get; set; }
    //public Chunk CurrentChunk { get; set; }

    public Vector3 Position => transform.position;
    // Start is called before the first frame update
    void Awake()
    {
        Engine = FindObjectOfType<VoxelEngine>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateChunk(Chunk chunk)
    {
        MeshFilter.mesh = chunk.AssignMesh();
        MeshCollider.sharedMesh = MeshFilter.mesh;
        // if (!chunk.MeshCreated) chunk.AssignMesh();
        //
        // MeshFilter.mesh = chunk.mesh;
    }
}