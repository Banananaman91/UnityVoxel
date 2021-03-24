using System;
using UnityEngine;

namespace VoxelTerrain.Mouse
{
    public class MouseInteraction : MonoBehaviour
    {
        [SerializeField] private VoxelInteraction _voxelInteraction;
        
        private void Update()
        {
            switch (_voxelInteraction.Shape)
            {
                case FlattenShape.Single:
                    if (Input.GetMouseButton(0)) _voxelInteraction.DestroyVoxel();
                    else if (Input.GetMouseButton(1)) _voxelInteraction.CreateVoxel();
                    break;
                case FlattenShape.Square:
                    if (Input.GetMouseButtonDown(0)) _voxelInteraction.DestroyVoxel();
                    else if (Input.GetMouseButtonDown(1)) _voxelInteraction.CreateVoxel();
                    break;
                case FlattenShape.Circular:
                    if (Input.GetMouseButtonDown(0)) _voxelInteraction.DestroyVoxel();
                    else if (Input.GetMouseButtonDown(1)) _voxelInteraction.CreateVoxel();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
