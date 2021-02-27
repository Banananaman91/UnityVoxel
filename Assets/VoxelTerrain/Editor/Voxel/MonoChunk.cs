﻿using System;
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

    private Chunk _chunk;

    public Vector3 Position => transform.position;
    // Start is called before the first frame update
    void Awake()
    {
        Engine = FindObjectOfType<VoxelEngine>();
    }
}
