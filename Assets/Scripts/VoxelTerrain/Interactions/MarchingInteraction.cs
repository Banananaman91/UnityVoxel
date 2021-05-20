using System;
using UnityEngine;
using VoxelTerrain.Engine;
using VoxelTerrain.Grid;

namespace VoxelTerrain.Interactions
{
    public class MarchingInteraction : MonoBehaviour
    {
        private Camera CamMain => Camera.main;
        [SerializeField] private VoxelEngine _engine;

        private void Awake()
        {
            if (!_engine) _engine = FindObjectOfType<VoxelEngine>();
        }

        private void Update()
        {
            if (!Input.GetKey(KeyCode.Mouse1)) return;
            
            var ray = CamMain.ViewportPointToRay(CamMain.ScreenToViewportPoint(Input.mousePosition));
        
            if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            //If we have hit something, snap the hit position to a voxel position
            var hitPos = GridSnapper.SnapToGrid(hit.point);
            hitPos.y -= 1;

            var chunk = _engine.WorldData[hitPos.x, hitPos.y, hitPos.z];
            var voxelPos = hitPos - chunk.Position;
            var voxel = chunk[voxelPos.x, voxelPos.y, voxelPos.z];
            voxel.Value -= 0.5f;
            chunk.SetVoxel(voxelPos, voxel);
            chunk.SetMesh(chunk.Position);
            _engine.WorldData[hitPos.x, hitPos.y, hitPos.z] = chunk;
        }
    }
}
