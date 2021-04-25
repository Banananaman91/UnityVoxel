﻿using System;
using UnityEngine;
using VoxelTerrain.Voxel;

namespace VoxelTerrain.Interactions
{
    public class MouseInteraction : MonoBehaviour
    {
        [SerializeField] private VoxelInteraction _voxelInteraction;
        
        //Basic mouse input for destroying and creating voxels
        //Switch case used to prevent certain methods running continuously
        private void Update()
        {
            switch (_voxelInteraction.Shape)
            {
                case FlattenShape.Single:
                    if (Input.GetMouseButton(0))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Default);
                        _voxelInteraction.EditVoxels();
                    }
                    else if (Input.GetMouseButton(1))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Grass);
                        _voxelInteraction.EditVoxels();
                    }
                    break;
                case FlattenShape.Square:
                    if (Input.GetMouseButtonDown(0))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Default);
                        _voxelInteraction.EditVoxels();
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Grass);
                        _voxelInteraction.EditVoxels();
                    }
                    break;
                case FlattenShape.Circular:
                    if (Input.GetMouseButtonDown(0))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Default);
                        _voxelInteraction.EditVoxels();
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Grass);
                        _voxelInteraction.EditVoxels();
                    }
                    break;
                case FlattenShape.Sphere:
                    if (Input.GetMouseButtonDown(0))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Default);
                        _voxelInteraction.EditVoxels();
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        _voxelInteraction.SetVoxelType(VoxelType.Grass);
                        _voxelInteraction.EditVoxels();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
